// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Line info object contains an immutable snapshot of a line's status.
/// The line info contains all the publicly available information about a line, which does not include the line value.
/// The line must be requested to access the line value.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class LineInfo : LibGpiodProxyBase
{
    private readonly LineInfoSafeHandle _handle;

    /// <summary>
    /// Constructor for a line-info-proxy object that points to an existing gpiod line-info object using a safe handle.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html"/>
    public LineInfo(LineInfoSafeHandle handle)
        : base(handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Copies an existing gpiod line-info object and returns a new line-info-proxy object.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public LineInfo Copy()
    {
        var handle = CallLibgpiod(() => LibgpiodV2.gpiod_line_info_copy(_handle));

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not copy line info: {LastErr.GetMsg()}");
        }

        return new LineInfo(handle);
    }

    /// <summary>
    /// Get the offset of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga475a19156d5f4cc301a3c08244561b31"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public Offset GetOffset()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_offset(_handle));
    }

    /// <summary>
    /// Get the name of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga840f2441b570bdef1d6ed48549aab822"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string? GetName()
    {
        return CallLibgpiod(() =>
        {
            IntPtr namePtr = LibgpiodV2.gpiod_line_info_get_name(_handle);

            if (namePtr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringAuto(namePtr) ?? throw new GpiodException($"Could not get name from line info: {LastErr.GetMsg()}");
        });
    }

    /// <summary>
    /// Check if the line is in use
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#gaa396a0742e7ddec1a2132e5a7e5ad4a3"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public bool GetIsUsed()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_is_used(_handle));
    }

    /// <summary>
    /// Get the name of the consumer of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga6a0cdc283fb52809a1fa033e9f84559a"/>
    /// <returns>The name of the consumer or <c>null</c> if the line has no consumer</returns>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string? GetConsumer()
    {
        return CallLibgpiod(() =>
        {
            IntPtr namePtr = LibgpiodV2.gpiod_line_info_get_consumer(_handle);

            if (namePtr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringAuto(namePtr) ?? throw new GpiodException($"Could not get consumer from line info: {LastErr.GetMsg()}");
        });
    }

    /// <summary>
    /// Get the direction setting of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga4de32ec5cfe5fc71036f004eaa82b60e"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodLineDirection GetDirection()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_direction(_handle));
    }

    /// <summary>
    /// Get the edge detection setting of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#gac0e128b53298d16af4e31cde1972bfe0"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodLineEdge GetEdgeDetection()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_edge_detection(_handle));
    }

    /// <summary>
    /// Get the bias setting of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#gaf687e04d662e415462607a478a09378f"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodLineBias GetBias()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_bias(_handle));
    }

    /// <summary>
    /// Get the drive setting of the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#gaf69559182170b0fb0c85cfd187f10e99"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodLineDrive GetDrive()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_drive(_handle));
    }

    /// <summary>
    /// Check if the logical value of the line is inverted compared to the physical
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga9af13bafcc842e51fd5f3254c172252d"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public bool GetIsActiveLow()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_is_active_low(_handle));
    }

    /// <summary>
    /// Check if the line is debounced (either by hardware or by the kernel software debouncer)
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga6837821a7c416b4942740917266d59bf"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public bool GetIsDebounced()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_is_debounced(_handle));
    }

    /// <summary>
    /// Get the debounce period of the line, in microseconds
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#gad7a9665eb5f0daeb5a097818ff6b4e08"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public TimeSpan GetDebouncePeriod()
    {
        return CallLibgpiod(() =>
        {
            ulong debouncePeriodMicroseconds = LibgpiodV2.gpiod_line_info_get_debounce_period_us(_handle);
            return TimeSpan.FromMilliseconds(debouncePeriodMicroseconds / 1000f);
        });
    }

    /// <summary>
    /// Get the event clock setting used for edge event timestamps for the line
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__info.html#ga436753e2a5e8f445b2d844ac1e45df25"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public GpiodLineClock GetEventClock()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_info_get_event_clock(_handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return new Snapshot(GetOffset(), GetName(), GetIsUsed(), GetConsumer(), GetDirection(), GetEdgeDetection(), GetBias(), GetDrive(),
            GetIsActiveLow(), GetIsDebounced(), GetDebouncePeriod(), GetEventClock());
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(Offset Offset, string? Name, bool IsUsed, string? Consumer, GpiodLineDirection Direction,
        GpiodLineEdge EdgeDetection, GpiodLineBias Bias, GpiodLineDrive Drive, bool IsActiveLow, bool IsDebounced, TimeSpan DebouncePeriod,
        GpiodLineClock EventClock)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return
                $"{nameof(Offset)}: {Offset}, {nameof(Name)}: {Name}, {nameof(IsUsed)}: {IsUsed}, {nameof(Consumer)}: {Consumer}, {nameof(Direction)}: {Direction}, {nameof(EdgeDetection)}: {EdgeDetection}, {nameof(Bias)}: {Bias}, {nameof(Drive)}: {Drive}, {nameof(IsActiveLow)}: {IsActiveLow}, {nameof(IsDebounced)}: {IsDebounced}, {nameof(DebouncePeriod)}: {DebouncePeriod}, {nameof(EventClock)}: {EventClock}";
        }
    }
}
