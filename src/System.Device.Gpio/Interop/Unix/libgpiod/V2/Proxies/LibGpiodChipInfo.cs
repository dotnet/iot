// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// The chip info contains all the publicly available information about a chip.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class LibGpiodChipInfo : LibGpiodProxyBase
{
    private readonly int _chipNumber;
    private readonly ChipInfoSafeHandle _handle;

    /// <summary>
    /// Constructor for a chip-info-proxy object that points to an existing gpiod chip-info object using a safe handle.
    /// </summary>
    /// <param name="chipNumber">The chip number (for identification)</param>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html"/>
    public LibGpiodChipInfo(int chipNumber, ChipInfoSafeHandle handle)
        : base(handle)
    {
        _chipNumber = chipNumber;
        _handle = handle;
    }

    public int ChipNumber => _chipNumber;

    /// <summary>
    /// Get the name of the chip as represented in the kernel
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#gafa15c848b7dd866ea55b4b39da74d6ad"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetName()
    {
        return CallLibgpiod(() => Marshal.PtrToStringAuto(LibgpiodV2.gpiod_chip_info_get_name(_handle)) ??
            throw new GpiodException($"Could not get name from chip info: {LastErr.GetMsg()}"));
    }

    /// <summary>
    /// Get the label of the chip as represented in the kernel
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#ga6d9fa1a356c45ea3c2605667201aa769"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetLabel()
    {
        return CallLibgpiod(() => Marshal.PtrToStringAuto(LibgpiodV2.gpiod_chip_info_get_label(_handle)) ??
            throw new GpiodException($"Could not get label from chip info: {LastErr.GetMsg()}"));
    }

    /// <summary>
    /// Get the number of lines exposed by the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#gad7c1bdfa5d489e7ecd57a705d4fa217a"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetNumLines()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_chip_info_get_num_lines(_handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public GpioChipInfo MakeSnapshot()
    {
        return new GpioChipInfo(_chipNumber, GetName(), GetLabel(), GetNumLines());
    }
}
