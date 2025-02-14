// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// A Line Request is similar in concept to a "reservation" of the requested line/s.
/// In order to get a Request object, a RequestConfig object has to be sent to a chip,
/// that denies or grants the request, returning a new Request object.
/// This object provides exclusive usage, i.e. reading or setting lines state.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#details"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class LineRequest : LibGpiodProxyBase
{
    private readonly LineRequestSafeHandle _handle;

    private readonly object _signalPipeWriteSideLock = new();
    private int? _signalPipeWriteSide;

    /// <summary>
    /// Constructor for a line-request-proxy object that points to an existing gpiod line-request object using a safe handle.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html"/>
    public LineRequest(LineRequestSafeHandle handle)
        : base(handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Indicates whether this request can be used to interact with.
    /// </summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>
    /// Get the number of lines in the request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga4e77f97baf50ba2bc45747ab35813053"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public int GetNumRequestedLines()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_request_get_num_requested_lines(_handle));
    }

    /// <summary>
    /// Get the offsets of the lines in the request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga15e16ace4b3abc7ce6174b23a0f1b4f7"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IEnumerable<Offset> GetRequestedOffsets()
    {
        return CallLibgpiod(() =>
        {
            int numRequestedLines = GetNumRequestedLines();
            uint[] requestedOffsets = new uint[numRequestedLines];
            int nStored = LibgpiodV2.gpiod_line_request_get_requested_offsets(_handle, requestedOffsets, requestedOffsets.Length);
            Array.Resize(ref requestedOffsets, nStored);
            return requestedOffsets.Convert();
        });
    }

    /// <summary>
    /// Get the value of a single requested line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gac1bbffb471768425c85d231077f1da0a"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineValue GetValue(Offset offset)
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_request_get_value(_handle, offset));
    }

    /// <summary>
    /// Get the values of a subset of requested lines
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga5c9ff52972737a0c6e72f9d580421e39"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IEnumerable<GpiodLineValue> GetValuesSubset(IEnumerable<Offset> offsets)
    {
        return CallLibgpiod(() =>
        {
            uint[] offsetsArr = offsets.ToArray().Convert();
            var offsetValues = new GpiodLineValue[offsetsArr.Length];
            int result = LibgpiodV2.gpiod_line_request_get_values_subset(_handle, offsetValues.Length, offsetsArr, offsetValues);
            if (result < 0)
            {
                throw new GpiodException($"Could not get multiple line values from request: {LastErr.GetMsg()}");
            }

            return offsetValues;
        });
    }

    /// <summary>
    /// Get the values of all requested lines
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga4b2ef066faf905e61f5b45f8f2c8f195"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IEnumerable<GpiodLineValue> GetValues()
    {
        return CallLibgpiod(() =>
        {
            int numRequestedLines = GetNumRequestedLines();
            var offsetValues = new GpiodLineValue[numRequestedLines];
            int result = LibgpiodV2.gpiod_line_request_get_values(_handle, offsetValues);
            if (result < 0)
            {
                throw new GpiodException($"Could not get all line values from request: {LastErr.GetMsg()}");
            }

            return offsetValues;
        });
    }

    /// <summary>
    /// Set the value of a single requested line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga22131b9472ea893194340683fa56d45b"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetValue(Offset offset, GpiodLineValue value)
    {
        CallLibpiodLocked(() =>
        {
            int result = LibgpiodV2.gpiod_line_request_set_value(_handle, offset, value);
            if (result < 0)
            {
                throw new GpiodException($"Could not set value '{value}' for offset '{offset}': {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Set the values of a subset of requested lines
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga5e48a32e554537a512bb3d5d04cd5c6a"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetValuesSubset(IEnumerable<(Offset _offset, GpiodLineValue _value)> valueByOffset)
    {
        CallLibpiodLocked(() =>
        {
            var tupleArr = valueByOffset.ToArray();
            uint[] offsets = tupleArr.Select(x => x._offset).ToArray().Convert();
            var values = tupleArr.Select(x => x._value).ToArray();
            int result = LibgpiodV2.gpiod_line_request_set_values_subset(_handle, offsets.Length, offsets, values);
            if (result < 0)
            {
                throw new GpiodException($"Could not set multiple values: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Set the values of all lines associated with a request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gaca3ed4c1ac47e233c8d6c1da82189b4f"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    /// <exception cref="ArgumentOutOfRangeException">Count of values does not match number of currently requested lines</exception>
    public void SetValues(IEnumerable<GpiodLineValue> values)
    {
        CallLibpiodLocked(() =>
        {
            int numRequestedLines = GetNumRequestedLines();
            var valArr = values.ToArray();
            if (valArr.Length != numRequestedLines)
            {
                throw new ArgumentOutOfRangeException(nameof(values),
                    $"Cannot set values because '{valArr.Length}' values were provided but '{numRequestedLines}' are currently requested");
            }

            int result = LibgpiodV2.gpiod_line_request_set_values(_handle, valArr);
            if (result < 0)
            {
                throw new GpiodException($"Could not set values: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Update the configuration of lines associated with a line request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gac105ea1e4d4dd82bd70fb224016b3ec5"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void ReconfigureLines(LineConfig lineConfig)
    {
        CallLibpiodLocked(() =>
        {
            int result = LibgpiodV2.gpiod_line_request_reconfigure_lines(_handle, lineConfig.Handle);
            if (result < 0)
            {
                throw new GpiodException($"Could not reconfigure lines: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get the file descriptor associated with a line request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga5c0dbdcd8608b76e77b78bca9a6b03d7"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public int GetFileDescriptor()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_request_get_fd(_handle));
    }

    /// <summary>
    /// Wait for edge events on any of the requested lines.
    /// </summary>
    /// <param name="timeout">Wait time limit. If set to 0, the function returns immediately. Defaults to 1 second.</param>
    /// <param name="waitIndefinitely">When set to true, <paramref name="timeout"/> is ignored and the function blocks indefinitely until an event becomes available.</param>
    /// <returns>
    /// 0 if wait timed out, 1 if an event is pending. q Lines must have edge detection set for edge events to be emitted. By default edge detection is disabled.
    /// </returns>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gaa50f3ac6cd2b3373f36ecd32603215bc"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public int WaitEdgeEvents(TimeSpan? timeout = null, bool waitIndefinitely = false)
    {
        return CallLibgpiod(() =>
        {
            timeout ??= TimeSpan.FromSeconds(1);
            long timeoutNs = waitIndefinitely ? -1 : (long)timeout.Value.TotalMilliseconds * 1_000_000;
            int result = LibgpiodV2.gpiod_line_request_wait_edge_events(_handle, timeoutNs);
            if (result < 0)
            {
                throw new GpiodException($"Could not wait for edge events: {LastErr.GetMsg()}");
            }

            return result;
        });
    }

    /// <summary>
    /// A more respectful version of <see cref="WaitEdgeEvents"/> that returns as soon as another thread signals it to i.e. this method
    /// does not stubbornly wait for the timeout or an event to appear, blocking any other call in between.
    /// </summary>
    /// <remarks>
    /// Uses epoll to wait for either edge events of the requests file descriptor or signals coming from <see cref="StopWaitingOnEdgeEvents"/>.
    /// For example when this method is waiting on edge events, and someone calls Close, a signal will be written to the signal file descriptor
    /// which triggers epoll to return, which allows releasing the lock to let the other thread through.
    /// </remarks>
    /// <returns>0 if wait timed out, 2 if wait got interrupted, 1 if an event is pending.</returns>
    public int WaitEdgeEventsRespectfully(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromMilliseconds(100);

        return CallLibgpiod(() =>
        {
            int requestFileDescriptor = GetFileDescriptor();
            int pollFileDescriptor = Interop.epoll_create(1);
            if (pollFileDescriptor < 0)
            {
                throw new GpiodException($"Error while waiting for edge events, epoll_create: {LastErr.GetMsg()}");
            }

            try
            {
                var requestEvent = new epoll_event
                {
                    events = PollEvents.EPOLLIN | PollEvents.EPOLLPRI, data = new epoll_data { fd = requestFileDescriptor }
                };

                int ret = Interop.epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_ADD, requestFileDescriptor, ref requestEvent);
                if (ret < 0)
                {
                    throw new GpiodException($"Error while waiting for edge events, epoll_ctl: {LastErr.GetMsg()}");
                }

                int[] signalPipe = new int[2];
                ret = Interop.pipe(signalPipe);
                if (ret < 0)
                {
                    throw new GpiodException($"Error while waiting for edge events, pipe: {LastErr.GetMsg()}");
                }

                int signalPipeReadSide = signalPipe[0];

                lock (_signalPipeWriteSideLock)
                {
                    _signalPipeWriteSide = signalPipe[1];
                }

                try
                {
                    var signalEvent = new epoll_event
                    {
                        events = PollEvents.EPOLLIN | PollEvents.EPOLLERR, data = new epoll_data { fd = signalPipeReadSide }
                    };

                    ret = Interop.epoll_ctl(pollFileDescriptor, PollOperations.EPOLL_CTL_ADD, signalPipeReadSide, ref signalEvent);
                    if (ret < 0)
                    {
                        throw new GpiodException($"Error while waiting for edge events, epoll_ctl: {LastErr.GetMsg()}");
                    }

                    using var eventBuffer = new UnmanagedArray<epoll_event>(2);
                    ret = Interop.epoll_wait(pollFileDescriptor, eventBuffer, 2, (int)timeout.Value.TotalMilliseconds);
                    if (ret < 0)
                    {
                        throw new GpiodException($"Error while waiting for edge events, epoll_wait: {LastErr.GetMsg()}");
                    }

                    bool isTimeout = ret == 0;

                    if (isTimeout)
                    {
                        return 0;
                    }

                    var events = eventBuffer.ReadToManagedArray();
                    for (int i = 0; i < events.Length; ++i)
                    {
                        if (events[i].data.fd == signalPipeReadSide)
                        {
                            return 2;
                        }

                        if (events[i].data.fd == requestFileDescriptor)
                        {
                            return 1;
                        }
                    }
                }
                finally
                {
                    lock (_signalPipeWriteSideLock)
                    {
                        Interop.close(signalPipeReadSide);
                        if (_signalPipeWriteSide != null)
                        {
                            Interop.close(_signalPipeWriteSide.Value);
                            _signalPipeWriteSide = null;
                        }
                    }
                }
            }
            finally
            {
                Interop.close(pollFileDescriptor);
            }

            return 0;
        });
    }

    /// <summary>
    /// Stops <see cref="WaitEdgeEventsRespectfully"/> waiting for events.
    /// </summary>
    private void StopWaitingOnEdgeEvents()
    {
        lock (_signalPipeWriteSideLock)
        {
            if (_signalPipeWriteSide == null)
            {
                return;
            }

            // the byte value is irrelevant, it is only used as a signal
            IntPtr signal = Marshal.AllocHGlobal(1);

            try
            {
                int ret = Interop.write(_signalPipeWriteSide.Value, signal, 1);
                if (ret < 0)
                {
                    throw new GpiodException($"Error signaling to stop waiting for edge events: write: {LastErr.GetMsg()}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(signal);
                lock (_signalPipeWriteSideLock)
                {
                    if (_signalPipeWriteSide != null)
                    {
                        Interop.close(_signalPipeWriteSide.Value);
                        _signalPipeWriteSide = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Read a number of edge events from a line request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gae260e953a25f602a57673bc42460ae63"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public int ReadEdgeEvents(EdgeEventBuffer edgeEventBuffer)
    {
        return CallLibgpiod(() =>
        {
            int nReadEvents = LibgpiodV2.gpiod_line_request_read_edge_events(_handle, edgeEventBuffer.Handle, edgeEventBuffer.Capacity);
            if (nReadEvents < 0)
            {
                throw new GpiodException($"Could not read edge events: {LastErr.GetMsg()}");
            }

            return nReadEvents;
        });
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot(Chip chip)
    {
        var requestedOffsets = GetRequestedOffsets();
        return new Snapshot(GetNumRequestedLines(), requestedOffsets.Select(offset =>
        {
            using LineInfo lineInfo = chip.GetLineInfo(offset);
            return lineInfo.MakeSnapshot();
        }));
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(int NumRequestedLines, IEnumerable<LineInfo.Snapshot> RequestedLines)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(NumRequestedLines)}: {NumRequestedLines}, {nameof(RequestedLines)}: {string.Join("\n", RequestedLines)}";
        }
    }

    protected override void Dispose(bool disposeManagedResources)
    {
        IsAlive = false;
        StopWaitingOnEdgeEvents();
        base.Dispose(disposeManagedResources);
    }
}
