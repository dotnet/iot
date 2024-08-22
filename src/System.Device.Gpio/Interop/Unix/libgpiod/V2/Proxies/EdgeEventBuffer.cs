// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// An edge event object contains information about a single line edge event. It contains the event type, timestamp and the offset of the line on
/// which the event occurred as well as two sequence numbers (global for all lines in the associated request and local for this line only).
/// Edge events are stored into an edge-event buffer object to improve performance and to limit the number of memory allocations when a large number
/// of events are being read.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class EdgeEventBuffer : LibGpiodProxyBase
{
    internal EdgeEventBufferSafeHandle Handle { get; }
    public int Capacity { get; }

    /// <summary>
    /// Constructor for a edge-event-buffer-proxy object. This call will create a new gpiod edge-event-buffer object.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <param name="capacity">The capacity of the event buffer.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga4249b081f27908f7b8365dd897673e21"/>
    public EdgeEventBuffer(EdgeEventBufferSafeHandle handle, int capacity)
        : base(handle)
    {
        Handle = handle;
        Capacity = capacity;
    }

    /// <summary>
    /// Get the capacity (the max number of events that can be stored) of the event buffer
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gab969a727573749810c16470a163c091b"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetCapacity()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_edge_event_buffer_get_capacity(Handle));
    }

    /// <summary>
    /// Get an event stored in the buffer
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga3d5e3b2f0ca992e4d39df02202ff9458"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public EdgeEvent GetEvent(ulong index)
    {
        return CallLibgpiod(() =>
        {
            using EdgeEventNotFreeable edgeEventHandle = LibgpiodV2.gpiod_edge_event_buffer_get_event(Handle, index);
            // Since events are tied to the buffer instance, different threads may not operate on the buffer and any associated events at the same
            // time. Events can be copied using ::gpiod_edge_event_copy in order to create a standalone objects - which each may safely be used from
            // a different thread concurrently.
            EdgeEventSafeHandle edgeEventCopyHandle = LibgpiodV2.gpiod_edge_event_copy(edgeEventHandle);
            return new EdgeEvent(edgeEventCopyHandle);
        });
    }

    /// <summary>
    /// Get the number of events a buffer has stored
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gaf3bda00286ba4de94a4186c0ad61d255"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetNumEvents()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_edge_event_buffer_get_num_events(Handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return new Snapshot(GetCapacity(), GetNumEvents());
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(int Capacity, int NumEvents)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Capacity)}: {Capacity}, {nameof(NumEvents)}: {NumEvents}";
        }
    }
}
