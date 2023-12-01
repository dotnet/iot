// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Device.Gpio.Interop.Unix.libgpiod.V2.Binding.Enums;
using System.Device.Gpio.Interop.Unix.libgpiod.V2.Binding.Handles;
using System.Device.Gpio.Interop.Unix.libgpiod.V2.ValueTypes;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.V2.Proxies;

/// <summary>
/// An edge event object contains information about a single line edge event. It contains the event type, timestamp and the offset of the line on
/// which the event occurred as well as two sequence numbers (global for all lines in the associated request and local for this line only).
/// Edge events are stored into an edge-event buffer object to improve performance and to limit the number of memory allocations when a large number
/// of events are being read.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html"/>
internal class EdgeEvent : LibGpiodProxyBase
{
    private readonly EdgeEventSafeHandle _handle;

    /// <summary>
    /// Constructor for a edge-event-proxy object that points to an existing gpiod edge-event object using a safe handle.
    /// </summary>
    public EdgeEvent(EdgeEventSafeHandle handle)
    {
        if (handle.IsInvalid)
        {
            throw new ArgumentOutOfRangeException(nameof(handle), "Invalid handle");
        }

        _handle = handle;
    }

    /// <summary>
    /// Copies an existing gpiod edge-event object and returns a new edge-event-proxy object.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public EdgeEvent Copy()
    {
        var handle = TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_copy(_handle));

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not copy edge event: {LastErr.GetMsg()}");
        }

        return new EdgeEvent(handle);
    }

    /// <summary>
    /// Get the event type
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gaf74a2865b2c703616bb8f90d4885af60"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodEdgeEventType GetEventType()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_get_event_type(_handle));
    }

    /// <summary>
    /// Get the timestamp of the event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga84d5be0e6994fde7b5c660a6c9f2ac0c"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public ulong GetTimestampNs()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_get_timestamp_ns(_handle));
    }

    /// <summary>
    /// Get the offset of the line which triggered the event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga3ff41bf3245f0cd717afa3b7f6ad2649"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public Offset GetLineOffset()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_get_line_offset(_handle));
    }

    /// <summary>
    /// Get the global sequence number of the event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gae25e9202e25fc738baaae9d5788899d1"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public SequenceNumber GetGlobalSequenceNumber()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_get_global_seqno(_handle));
    }

    /// <summary>
    /// Get the event sequence number specific to the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gaa3deb45d0acc1b2cf8e81506559bc145"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public SequenceNumber GetLineSequenceNumber()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_get_line_seqno(_handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return new Snapshot(GetEventType(), GetTimestampNs(), GetLineOffset(), GetGlobalSequenceNumber(), GetLineSequenceNumber());
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(GpiodEdgeEventType Type, ulong TimestampNs, Offset LineOffset, SequenceNumber GlobalSequenceNumber,
        SequenceNumber LineSequenceNumber)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(TimestampNs)}: {TimestampNs}, {nameof(LineOffset)}: {LineOffset}, {nameof(GlobalSequenceNumber)}: {GlobalSequenceNumber}, {nameof(LineSequenceNumber)}: {LineSequenceNumber}";
        }
    }
}
