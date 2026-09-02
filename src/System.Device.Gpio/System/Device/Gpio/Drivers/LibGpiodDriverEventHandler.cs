// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Libgpiod.V1;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Drivers;

internal sealed class LibGpiodDriverEventHandler : IDisposable
{
    private const int ERROR_CODE_EINTR = 4; // Interrupted system call

    // struct gpioevent_data from the kernel GPIO uAPI: __u64 timestamp; __u32 id.
    // This is the kernel ABI, not libgpiod's time64-dependent wrapper, which is why reading it
    // directly sidesteps the problem. timestamp is always at offset 0 and id at offset 8; only
    // the trailing alignment differs, so the kernel returns a 12- or 16-byte record depending on
    // the reading process. Accept either, and parse only the first 12 bytes.
    private const int EventRecordBufferSize = 16;
    private const int EventRecordPackedSize = 12;
    private const int EventRecordAlignedSize = 16;
    private const int EventIdOffset = 8;
    private const int GpioEventRisingEdge = 1;

    // Backstop only: cancellation comes from the self-pipe. This bounds disposal if a wake ever
    // fails to arrive, at one wakeup per second per line instead of the twenty it used to take.
    private const int PollBackstopMilliseconds = 1000;

    private static readonly string s_consumerName = Process.GetCurrentProcess().ProcessName;

    // gpiod_line_event_get_fd was added in libgpiod 1.0.1. Older libraries fall back to the
    // gpiod_line_event_wait path, which is correct on them: those releases long predate the
    // 64-bit time_t transition that makes the managed struct timespec the wrong size.
    private static bool s_lineEventFdUnavailable;

    public event PinChangeEventHandler? ValueRising;
    public event PinChangeEventHandler? ValueFalling;

    private readonly int _pinNumber;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _task;

    // Self-pipe used to wake the detection thread out of poll() when disposing, so that
    // Dispose() does not have to wait for an edge. -1 when the legacy path is in use.
    private readonly int _cancellationReadFd = -1;
    private readonly int _cancellationWriteFd = -1;

    private volatile bool _disposing;

    // 0 while live, 1 once a caller has taken ownership of disposal. Guarantees exactly one
    // owner closes the pipe descriptors, so a concurrent second Dispose() cannot close a
    // descriptor number that the kernel has since handed to something else.
    private int _disposeOwned;

    public LibGpiodDriverEventHandler(int pinNumber, LineHandle safeLineHandle)
    {
        _pinNumber = pinNumber;
        _cancellationTokenSource = new CancellationTokenSource();
        SubscribeForEvent(safeLineHandle);

        // Dispose() is never reached for a half-constructed handler, so everything acquired
        // after the line is requested has to be rolled back by hand.
        bool pipeCreated = false;
        try
        {
            int lineEventFd = TryGetLineEventFd(safeLineHandle);
            if (lineEventFd < 0)
            {
                // libgpiod predating 1.0.1 has no event descriptor, leaving only the legacy wait
                // path. That path passes a managed struct timespec, which is known-correct only
                // where sizeof(long) == sizeof(time_t). That holds on 64-bit; on 32-bit an old
                // libgpiod source rebuilt against a _TIME_BITS=64 libc would be handed undersized
                // structures, so refuse rather than risk the corruption this change exists to fix.
                if (IntPtr.Size < sizeof(long))
                {
                    throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.LibGpiodVersionTooOld);
                }

                _task = InitializeLegacyEventDetectionTask(_cancellationTokenSource.Token, safeLineHandle);
                return;
            }

            int[] cancellationPipe = new int[2];
            if (Interop.pipe2(cancellationPipe, Interop.O_CLOEXEC) < 0)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.RequestEventError, ExceptionHelper.GetLastErrorMessage(), _pinNumber);
            }

            _cancellationReadFd = cancellationPipe[0];
            _cancellationWriteFd = cancellationPipe[1];
            pipeCreated = true;

            _task = InitializeEventDetectionTask(_cancellationTokenSource.Token, lineEventFd);
        }
        catch
        {
            if (pipeCreated)
            {
                Interop.close(_cancellationReadFd);
                Interop.close(_cancellationWriteFd);
            }

            // Undo the event request this constructor made. ReleaseLock does not invalidate the
            // handle, so the driver keeps ownership of the LineHandle it passed in.
            safeLineHandle.ReleaseLock();
            throw;
        }
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    private void SubscribeForEvent(LineHandle pinHandle)
    {
        int eventSuccess = LibgpiodV1.gpiod_line_request_both_edges_events(pinHandle.Handle, s_consumerName);

        if (eventSuccess < 0)
        {
            throw ExceptionHelper.GetIOException(ExceptionResource.RequestEventError, ExceptionHelper.GetLastErrorMessage(), _pinNumber);
        }
    }

    /// <summary>
    /// Returns the line event descriptor, or -1 when the installed libgpiod predates 1.0.1 and
    /// does not provide one, in which case the caller uses the legacy wait loop.
    /// </summary>
    private int TryGetLineEventFd(LineHandle safeLineHandle)
    {
        if (s_lineEventFdUnavailable)
        {
            return -1;
        }

        int lineEventFd;
        try
        {
            lineEventFd = LibgpiodV1.gpiod_line_event_get_fd(safeLineHandle.Handle);
        }
        catch (EntryPointNotFoundException)
        {
            s_lineEventFdUnavailable = true;
            return -1;
        }

        if (lineEventFd < 0)
        {
            throw ExceptionHelper.GetIOException(ExceptionResource.RequestEventError, ExceptionHelper.GetLastErrorMessage(), _pinNumber);
        }

        return lineEventFd;
    }

    private Task InitializeEventDetectionTask(CancellationToken token, int lineEventFd)
    {
        // LongRunning so the default scheduler gives this a dedicated thread instead of a
        // thread-pool worker: the loop blocks in poll() for the lifetime of the subscription,
        // which would otherwise hold a pool thread permanently, once per subscribed pin.
        // Keeping it a Task preserves exception capture and the join in Dispose().
        return Task.Factory.StartNew(() =>
        {
            // POLLIN | POLLPRI mirrors the mask libgpiod's own multi-line wait requests.
            pollfd[] descriptors =
            {
                new pollfd { fd = lineEventFd, events = PollFlags.POLLIN | PollFlags.POLLPRI },
                new pollfd { fd = _cancellationReadFd, events = PollFlags.POLLIN }
            };

            IntPtr eventRecord = Marshal.AllocHGlobal(EventRecordBufferSize);
            try
            {
                while (!(token.IsCancellationRequested || _disposing))
                {
                    descriptors[0].revents = PollFlags.None;
                    descriptors[1].revents = PollFlags.None;

                    int ready = Interop.poll(descriptors, (nuint)descriptors.Length, PollBackstopMilliseconds);
                    if (ready < 0)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == ERROR_CODE_EINTR)
                        {
                            // ignore Interrupted system call error and retry
                            continue;
                        }

                        string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                        string errorInfo = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventWaitError, errorInfo, _pinNumber);
                    }

                    if (ready == 0)
                    {
                        // Backstop expiry: re-check the loop condition and wait again.
                        continue;
                    }

                    if (descriptors[1].revents != PollFlags.None)
                    {
                        // Disposing: stop without waiting for an edge.
                        break;
                    }

                    if ((descriptors[0].revents & (PollFlags.POLLERR | PollFlags.POLLHUP | PollFlags.POLLNVAL)) != 0)
                    {
                        // Not a normal shutdown: the line is requested for the lifetime of this
                        // handler, so an unusable descriptor is a lifetime error. Report it the
                        // way a wait failure was reported before, rather than stopping quietly.
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventWaitError, descriptors[0].revents.ToString(), _pinNumber);
                    }

                    if ((descriptors[0].revents & (PollFlags.POLLIN | PollFlags.POLLPRI)) == 0)
                    {
                        continue;
                    }

                    int read = Interop.read(lineEventFd, eventRecord, EventRecordBufferSize);
                    if (read < 0)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == ERROR_CODE_EINTR)
                        {
                            continue;
                        }

                        throw ExceptionHelper.GetIOException(ExceptionResource.EventReadError, ExceptionHelper.GetLastErrorMessage());
                    }

                    if (read != EventRecordPackedSize && read != EventRecordAlignedSize)
                    {
                        // libgpiod itself treats a partial event record as EIO. Anything other
                        // than one whole record (12 bytes packed, 16 bytes aligned) is an error,
                        // including 0, which would otherwise spin.
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventReadError, read.ToString());
                    }

                    int eventId = Marshal.ReadInt32(eventRecord, EventIdOffset);
                    PinEventTypes eventType = eventId == GpioEventRisingEdge ? PinEventTypes.Rising : PinEventTypes.Falling;
                    OnPinValueChanged(new PinValueChangedEventArgs(eventType, _pinNumber), eventType);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(eventRecord);
            }
        },
        token,
        TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
        TaskScheduler.Default);
    }

    /// <summary>
    /// Pre-1.0.1 libgpiod path, unchanged from before the event descriptor was used. Correct on
    /// those releases: they predate 64-bit time_t, so the managed struct timespec matches.
    /// </summary>
    private Task InitializeLegacyEventDetectionTask(CancellationToken token, LineHandle pinHandle)
    {
        return Task.Run(() =>
        {
            while (!(token.IsCancellationRequested || _disposing))
            {
                // WaitEventResult can be TimedOut, EventOccured or Error, in case of TimedOut will continue waiting
                TimeSpec timeout = new TimeSpec
                {
                    TvSec = new nint(0),
                    TvNsec = new nint(50_000_000)
                };

                WaitEventResult waitResult = LibgpiodV1.gpiod_line_event_wait(pinHandle.Handle, ref timeout);
                if (waitResult == WaitEventResult.Error)
                {
                    // Can't use ExceptionHelper.GetLastErrorMessage() here because we need the error code
                    // for the EINTR check. GetLastErrorMessage() would call GetLastWin32Error() internally,
                    // and we can't call GetLastWin32Error() twice as subsequent calls might return different values.
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == ERROR_CODE_EINTR)
                    {
                        // ignore Interrupted system call error and retry
                        continue;
                    }

                    string errorMessage = Marshal.GetLastPInvokeErrorMessage();
                    string errorInfo = string.IsNullOrWhiteSpace(errorMessage) ? errorCode.ToString() : $"{errorCode} ({errorMessage})";
                    throw ExceptionHelper.GetIOException(ExceptionResource.EventWaitError, errorInfo, _pinNumber);
                }

                if (waitResult == WaitEventResult.EventOccured)
                {
                    GpioLineEvent eventResult = new GpioLineEvent();
                    int checkForEvent = LibgpiodV1.gpiod_line_event_read(pinHandle.Handle, ref eventResult);
                    if (checkForEvent == -1)
                    {
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventReadError, ExceptionHelper.GetLastErrorMessage());
                    }

                    PinEventTypes eventType = eventResult.event_type == 1 ? PinEventTypes.Rising : PinEventTypes.Falling;
                    this?.OnPinValueChanged(new PinValueChangedEventArgs(eventType, _pinNumber), eventType);
                }
            }
        }, token);
    }

    public void OnPinValueChanged(PinValueChangedEventArgs args, PinEventTypes detectionOfEventTypes)
    {
        if (detectionOfEventTypes == PinEventTypes.Rising && args.ChangeType == PinEventTypes.Rising)
        {
            ValueRising?.Invoke(this, args);
        }

        if (detectionOfEventTypes == PinEventTypes.Falling && args.ChangeType == PinEventTypes.Falling)
        {
            ValueFalling?.Invoke(this, args);
        }
    }

    public bool IsCallbackListEmpty()
    {
        return ValueRising == null && ValueFalling == null;
    }

    public void Dispose()
    {
        // The first caller owns signalling and the descriptors, but every caller waits for the
        // detection task: returning early would let a containing driver release the line handle
        // while the task is still polling its descriptor.
        bool disposeOwner = Interlocked.Exchange(ref _disposeOwned, 1) == 0;
        if (disposeOwner)
        {
            _disposing = true;
            _cancellationTokenSource.Cancel();
            WakeDetectionTask();
        }

        try
        {
            _task.GetAwaiter().GetResult();
        }
        catch (TaskCanceledException)
        {
            // ignore cancellation exception
        }
        finally
        {
            if (disposeOwner && _cancellationReadFd >= 0)
            {
                Interop.close(_cancellationReadFd);
                Interop.close(_cancellationWriteFd);
            }
        }

        ValueRising = null;
        ValueFalling = null;
    }

    private void WakeDetectionTask()
    {
        if (_cancellationWriteFd < 0)
        {
            // Legacy path: the loop notices _disposing at its next 50 ms timeout instead.
            return;
        }

        IntPtr wakeByte = Marshal.AllocHGlobal(1);
        try
        {
            Marshal.WriteByte(wakeByte, 0);

            // An interrupted write is retried, because a lost wake would otherwise delay
            // disposal. Any other write failure is deliberately not fatal: the backstop timeout
            // bounds disposal to one second, which is preferable to throwing from Dispose().
            while (Interop.write(_cancellationWriteFd, wakeByte, 1) < 0
                   && Marshal.GetLastWin32Error() == ERROR_CODE_EINTR)
            {
            }
        }
        finally
        {
            Marshal.FreeHGlobal(wakeByte);
        }
    }
}
