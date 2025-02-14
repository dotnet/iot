// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Callers are notified about changes in a line's status due to GPIO uAPI calls.
/// Each info event contains information about the event itself (timestamp, type)
/// as well as a snapshot of line's status in the form of a line-info object.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class LineInfoEvent : LibGpiodProxyBase
{
    private readonly LineInfoEventSafeHandle _handle;

    /// <summary>
    /// Constructor for a line-info-event-proxy object that points to an existing gpiod line-info-event object using a safe handle.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html"/>
    public LineInfoEvent(LineInfoEventSafeHandle handle)
        : base(handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Get the event type of the status change event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html#ga24dd6128454beac4fe129cda1bda361e"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineInfoEventType GetEventType()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_info_event_get_event_type(_handle));
    }

    /// <summary>
    /// Get the timestamp of the event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html#ga2cc4923098c7832eeb72e869b6261a0b"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public ulong GetTimestampNs()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_info_event_get_timestamp_ns(_handle));
    }

    /// <summary>
    /// Get the snapshot of line-info associated with the event
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html#ga5b8e7faf72fe0e51512721b7583d0497"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public LineInfo GetLineInfo()
    {
        return CallLibgpiod(() =>
        {
            LineInfoSafeHandleNotFreeable lineInfoHandle = LibgpiodV2.gpiod_info_event_get_line_info(_handle);
            // Since the line-info object is tied to the event, different threads may not operate on the event and line-info at the same time.
            // The line-info can be copied using gpiod_line_info_copy in order to create a standalone object - which then may safely be used from a
            // different thread concurrently. See documentation 2.1
            LineInfoSafeHandle lineInfoCopyHandle = LibgpiodV2.gpiod_line_info_copy(lineInfoHandle);
            return new LineInfo(lineInfoCopyHandle);
        });
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        using LineInfo lineInfo = GetLineInfo();
        return new Snapshot(GetEventType(), GetTimestampNs(), lineInfo.MakeSnapshot());
    }

    public sealed record Snapshot(GpiodLineInfoEventType Type, ulong TimestampNs, LineInfo.Snapshot LineInfo)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(TimestampNs)}: {TimestampNs}, {nameof(LineInfo)}: {LineInfo}";
        }
    }
}
