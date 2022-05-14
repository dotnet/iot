// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Devices;
using Windows.Win32.Devices.Communication;
using Windows.Win32.Storage.FileSystem;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort
{
    internal class WindowsEventLoop
    {
        private const CLEAR_COMM_ERROR_FLAGS ErrorFlags = CLEAR_COMM_ERROR_FLAGS.CE_FRAME |
            CLEAR_COMM_ERROR_FLAGS.CE_OVERRUN |
            CLEAR_COMM_ERROR_FLAGS.CE_RXOVER |
            CLEAR_COMM_ERROR_FLAGS.CE_RXPARITY |
            (CLEAR_COMM_ERROR_FLAGS)0x100;  // SerialError.TXFull == 0x100

        // EV_RLSD is the CD changed of SerialPinChange
        private const COMM_EVENT_MASK PinChangedEvents = COMM_EVENT_MASK.EV_BREAK |
            COMM_EVENT_MASK.EV_RLSD |
            COMM_EVENT_MASK.EV_CTS |
            COMM_EVENT_MASK.EV_RING |
            COMM_EVENT_MASK.EV_DSR;

        // (int)(SerialData.Chars | SerialData.Eof);
        private const COMM_EVENT_MASK ReceivedEvents = COMM_EVENT_MASK.EV_RXCHAR | COMM_EVENT_MASK.EV_RXFLAG;

        private readonly SafeHandle _portHandle;
        private readonly ThreadPoolBoundHandle _threadPoolBound;
        private readonly SerialPort _serialPort;
        private COMM_EVENT_MASK _eventsOccurred;

        private bool _endEventLoop;

        public WindowsEventLoop(SerialPort serialPort, SafeHandle portHandle, ThreadPoolBoundHandle threadPoolBound)
        {
            _serialPort = serialPort;
            _portHandle = portHandle;
            _threadPoolBound = threadPoolBound;
        }

        public ManualResetEvent WaitCommEventWaitHandle { get; } = new ManualResetEvent(false);

        internal Task StartEventLoop()
        {
            // return Task.Factory.StartNew(s => ((WindowsEventLoop?)s)?.WaitForCommEvent(), _eventLoop,
            //    CancellationToken.None,
            //    TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
            return Task.Factory.StartNew(_ => WaitForCommEvent(), null, CancellationToken.None,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        internal void ShutdownEventLoop()
        {
            _endEventLoop = true;
        }

        internal unsafe void WaitForCommEvent()
        {
            bool doCleanup = false;
            NativeOverlapped* intOverlapped = null;
            while (!_endEventLoop)
            {
                WaitCommEventWaitHandle.Reset();
                var asyncResult = new WindowsSerialPortAsyncResult(WaitCommEventWaitHandle, null, null, false);

                // we're going to use _numBytes for something different in this loop.  In this case, both
                // freeNativeOverlappedCallback and this thread will decrement that value.  Whichever one decrements it
                // to zero will be the one to free the native overlapped.  This guarantees the overlapped gets freed
                // after both the callback and GetOverlappedResult have had a chance to use it.
                asyncResult._numBytes = 2;

                intOverlapped = _threadPoolBound.AllocateNativeOverlapped(FreeNativeOverlappedCallback, asyncResult, null);
                intOverlapped->EventHandle = WaitCommEventWaitHandle.SafeWaitHandle.DangerousGetHandle();

                fixed (COMM_EVENT_MASK* eventsOccurredPtr = &_eventsOccurred)
                {
                    if (WindowsHelpers.WaitCommEvent(_portHandle.DangerousGetHandle(),
                        eventsOccurredPtr, intOverlapped) == false)
                    {
                        var hr = (uint)Marshal.GetLastWin32Error();

                        // When a device is disconnected unexpectedly from a serial port, there appear to be
                        // at least three error codes Windows or drivers may return.
                        if (hr == (uint)WIN32_ERROR.ERROR_ACCESS_DENIED ||
                            hr == (uint)WIN32_ERROR.ERROR_BAD_COMMAND ||
                            hr == (uint)WIN32_ERROR.ERROR_DEVICE_REMOVED)
                        {
                            doCleanup = true;
                            break;
                        }

                        if (hr == (uint)WIN32_ERROR.ERROR_IO_PENDING)
                        {
                            int error;

                            // if we get IO pending, MSDN says we should wait on the WaitHandle, then call GetOverlappedResult
                            // to get the results of WaitCommEvent.
                            bool success = WaitCommEventWaitHandle.WaitOne();
                            Debug.Assert(success, $"WaitCommEventWaitHandle.WaitOne() returned error {Marshal.GetLastWin32Error()}");

                            do
                            {
                                // NOTE: GetOverlappedResult will modify the original pointer passed into WaitCommEvent.
                                success = WindowsHelpers.GetOverlappedResult(_portHandle.DangerousGetHandle(), intOverlapped, out _, false);
                                error = Marshal.GetLastWin32Error();
                            }
                            while (error == (uint)WIN32_ERROR.ERROR_IO_INCOMPLETE && !_endEventLoop && !success);

                            if (!success)
                            {
                                // Ignore ERROR_IO_INCOMPLETE and ERROR_INVALID_PARAMETER, because there's a chance we'll get
                                // one of those while shutting down
                                if (!((error == (uint)WIN32_ERROR.ERROR_IO_INCOMPLETE ||
                                    error == (uint)WIN32_ERROR.ERROR_INVALID_PARAMETER) && _endEventLoop))
                                {
                                    Debug.Fail("GetOverlappedResult returned error, we might leak intOverlapped memory" +
                                        error.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                        }
                        else if (hr != (uint)WIN32_ERROR.ERROR_INVALID_PARAMETER)
                        {
                            // ignore ERROR_INVALID_PARAMETER errors.  WaitCommError seems to return this
                            // when SetCommMask is changed while it's blocking (like we do in Dispose())
                            Debug.Fail("WaitCommEvent returned error " + hr);
                        }
                    }
                }

                if (!_endEventLoop)
                {
                    CallEvents(_eventsOccurred);
                }

                if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                {
                    _threadPoolBound.FreeNativeOverlapped(intOverlapped);
                }

            } // while (!ShutdownLoop)

            if (doCleanup)
            {
                // the rest will be handled in Dispose()
                _endEventLoop = true;
                _threadPoolBound.FreeNativeOverlapped(intOverlapped);
            }
        }

        private unsafe void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Extract the async result from overlapped structure
            var asyncResult = (WindowsSerialPortAsyncResult?)ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped);
            if (asyncResult == null)
            {
                Debug.WriteLine($"Unexpected null returned from {nameof(ThreadPoolBoundHandle.GetNativeOverlappedState)}");
                return;
            }

            if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
            {
                _threadPoolBound.FreeNativeOverlapped(pOverlapped);
            }
        }

        private unsafe void CallEvents(COMM_EVENT_MASK nativeEvents)
        {
            // EV_ERR includes only CE_FRAME, CE_OVERRUN, and CE_RXPARITY
            // To catch errors such as CE_RXOVER, we need to call CleanCommErrors bit more regularly.
            // EV_RXCHAR is perhaps too loose an event to look for overflow errors but a safe side to err...
            if ((nativeEvents & (COMM_EVENT_MASK.EV_ERR | COMM_EVENT_MASK.EV_RXCHAR)) != 0)
            {
                CLEAR_COMM_ERROR_FLAGS errorFlags;
                if (WindowsHelpers.ClearCommError(_portHandle.DangerousGetHandle(), out errorFlags, out COMSTAT _) == false)
                {
                    /*
                     * Comments from the old SerialPort class
                    // We don't want to throw an exception from the background thread which is un-catchable and hence tear down the process.
                    // At present we don't have a first class event that we can raise for this class of fatal errors. One possibility is
                    // to overload SeralErrors event to include another enum (perhaps CE_IOE) that we can use for this purpose.
                    // In the absence of that, it is better to eat this error silently than tearing down the process (lesser of the evil).
                    // This uncleared comm error will most likely blow up when the device is accessed by other APIs (such as Read) on the
                    // main thread and hence become known. It is bit roundabout but acceptable.
                    //
                    // Shutdown the event runner loop (probably bit drastic but we did come across a fatal error).
                    // Defer actual dispose chores until finalization though.
                    */

                    _endEventLoop = true;
                    Thread.MemoryBarrier();
                    return;
                }

                errorFlags = errorFlags & errorFlags;
                /*
                 * Comments from the old SerialPort class
                // TODO: what about CE_BREAK?  Is this the same as EV_BREAK?  EV_BREAK happens as one of the pin events,
                //       but CE_BREAK is returned from ClreaCommError.
                // TODO: what about other error conditions not covered by the enum?  Should those produce some other error?
                */
                if (errorFlags != 0)
                {
                    ThreadPool.QueueUserWorkItem(CallErrorEvents, errorFlags);
                }
            }

            // now look for pin changed and received events.
            if ((nativeEvents & PinChangedEvents) != 0)
            {
                ThreadPool.QueueUserWorkItem(CallPinEvents, nativeEvents);
            }

            if ((nativeEvents & ReceivedEvents) != 0)
            {
                ThreadPool.QueueUserWorkItem(CallReceiveEvents, nativeEvents);
            }
        }

        private void CallErrorEvents(object? state) => _serialPort?.TriggerErrors((int)state!);

        private void CallReceiveEvents(object? state) => _serialPort?.TriggerReceiveEvents((int)state!);

        private void CallPinEvents(object? state) => _serialPort?.TriggerPinEvents((int)state!);

    }
}

#pragma warning restore CA1416 // Validate platform compatibility
