// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Contains functions that are not part of any specific concept.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__misc.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class Miscellaneous
{
    /// <summary>
    /// Check if the file pointed to by path is a GPIO chip character device
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__misc.html#gace4957f84bc1a308e581cbc5ec71e96d"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static bool IsGpioChipDevice(string path)
    {
        return LibGpiodProxyBase.CallLibgpiod(() => LibgpiodV2.gpiod_is_gpiochip_device(Marshal.StringToHGlobalAuto(path)));
    }

    /// <summary>
    /// Get the API version of the library as a human-readable string
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__misc.html#gac7919e728ad7c9ba534ec543a8dbcac2"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static string GetApiVersion()
    {
        return LibGpiodProxyBase.CallLibgpiod(() =>
            Marshal.PtrToStringAuto(LibgpiodV2.gpiod_api_version()) ?? throw new GpiodException($"Could not get API version: {LastErr.GetMsg()}"));
    }
}
