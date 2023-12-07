// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// A Line Request is similar in concept to a "reservation" of the requested line/s.
/// In order to get a Request object, a RequestConfig object has to be sent to a chip,
/// that denies or grants the request, returning a new Request object.
/// This object provides exclusive usage, i.e. reading or setting lines state.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#details"/>
internal class LineRequest : LibGpiodProxyBase, IDisposable
{
    private readonly LineRequestSafeHandle _handle;

    /// <summary>
    /// Constructor for a line-request-proxy object that points to an existing gpiod line-request object using a safe handle.
    /// </summary>
    public LineRequest(LineRequestSafeHandle handle)
    {
        if (handle.IsInvalid)
        {
            throw new ArgumentOutOfRangeException(nameof(handle), "Invalid handle");
        }

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
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_line_request_get_num_requested_lines(_handle));
    }

    /// <summary>
    /// Get the offsets of the lines in the request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga15e16ace4b3abc7ce6174b23a0f1b4f7"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IEnumerable<Offset> GetRequestedOffsets()
    {
        return TryCallGpiodLocked(() =>
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
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_line_request_get_value(_handle, offset));
    }

    /// <summary>
    /// Get the values of a subset of requested lines
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#ga5c9ff52972737a0c6e72f9d580421e39"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IEnumerable<GpiodLineValue> GetValuesSubset(IEnumerable<Offset> offsets)
    {
        return TryCallGpiodLocked(() =>
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
        return TryCallGpiodLocked(() =>
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
        TryCallGpiodLocked(() =>
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
        TryCallGpiodLocked(() =>
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
        TryCallGpiodLocked(() =>
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
        TryCallGpiodLocked(() =>
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
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_line_request_get_fd(_handle));
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
        return TryCallGpiodLocked(() =>
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
    /// Read a number of edge events from a line request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__request.html#gae260e953a25f602a57673bc42460ae63"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public int ReadEdgeEvents(EdgeEventBuffer edgeEventBuffer)
    {
        return TryCallGpiodLocked(() =>
        {
            int nReadEvents = LibgpiodV2.gpiod_line_request_read_edge_events(_handle, edgeEventBuffer.Handle, edgeEventBuffer.GetCapacity());
            if (nReadEvents < 0)
            {
                throw new GpiodException($"Could not read edge events: {LastErr.GetMsg()}");
            }

            return nReadEvents;
        });
    }

    /// <summary>
    /// Releases the gpiod request object
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void Close()
    {
        TryCallGpiodLocked(_handle.Dispose);
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot(Chip chip)
    {
        var requestedOffsets = GetRequestedOffsets();
        return new Snapshot(GetNumRequestedLines(), requestedOffsets.Select(offset => chip.GetLineInfo(offset).MakeSnapshot()));
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

    #region Dispose

    private readonly object _isDisposedLock = new();
    private bool _isDisposed;

    public void Dispose()
    {
        lock (_isDisposedLock)
        {
            if (_isDisposed)
            {
                return;
            }

            IsAlive = false;
            Close();

            _isDisposed = true;
        }
    }

    #endregion
}
