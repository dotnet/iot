// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// An edge event object contains information about a single line edge event. It contains the event type, timestamp and the offset of the line on
/// which the event occurred as well as two sequence numbers (global for all lines in the associated request and local for this line only).
/// Edge events are stored into an edge-event buffer object to improve performance and to limit the number of memory allocations when a large number
/// of events are being read.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html"/>
internal class EdgeEventBuffer : LibGpiodProxyBase, IDisposable
{
    internal EdgeEventBufferSafeHandle Handle { get; }

    /// <summary>
    /// Constructor for a edge-event-buffer-proxy object. This call will create a new gpiod edge-event-buffer object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga4249b081f27908f7b8365dd897673e21"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public EdgeEventBuffer(int capacity = 10)
    {
        Handle = TryCallGpiod(() => LibgpiodV2.gpiod_edge_event_buffer_new(capacity));

        if (Handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new edge event buffer: {LastErr.GetMsg()}");
        }
    }

    /// <summary>
    /// Get the capacity (the max number of events that can be stored) of the event buffer
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#gab969a727573749810c16470a163c091b"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetCapacity()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_buffer_get_capacity(Handle));
    }

    /// <summary>
    /// Get an event stored in the buffer
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga3d5e3b2f0ca992e4d39df02202ff9458"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public EdgeEvent GetEvent(ulong index)
    {
        return TryCallGpiodLocked(() =>
        {
            EdgeEventNotFreeable edgeEventHandle = LibgpiodV2.gpiod_edge_event_buffer_get_event(Handle, index);
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
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_edge_event_buffer_get_num_events(Handle));
    }

    /// <summary>
    /// Releases the gpiod edge event buffer object
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void Close()
    {
        TryCallGpiodLocked(Handle.Dispose);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
    }
}
