// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal static class LibGpiodProxyFactory
{
    /// <summary>
    /// Creates a chip-proxy object. This call will try to open the chip.
    /// </summary>
    /// <param name="chipNumber">The chip number</param>
    /// <param name="devicePath">File system path to the chip device, e.g. '/dev/gpiochip4'</param>
    /// <exception cref="GpiodException">The chip does not exist or an unexpected error happened while opening chip</exception>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga25097f48949d0ac81e9ab341193da1a4"/>
    public static Chip CreateChip(int chipNumber, string devicePath)
    {
        var handle = LibGpiodProxyBase.CallLibgpiod(() => LibgpiodV2.gpiod_chip_open(Marshal.StringToHGlobalAuto(devicePath)));

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not open gpio chip at path '{devicePath}': {LastErr.GetMsg()}");
        }

        return new Chip(chipNumber, handle);
    }

    /// <summary>
    /// Creates a chip-proxy object. This call will try to open the chip.
    /// </summary>
    /// <param name="chipNumber">Number that will be translated to a device path for example 4 -> '/dev/gpiochip4'</param>
    /// <exception cref="GpiodException">The chip does not exist or an unexpected error happened while opening chip</exception>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga25097f48949d0ac81e9ab341193da1a4"/>
    public static Chip CreateChip(int chipNumber)
    {
        return CreateChip(chipNumber, $"/dev/gpiochip{chipNumber}");
    }

    /// <summary>
    /// Creates a edge-event-buffer-proxy object. This call will create a new gpiod edge-event-buffer object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga4249b081f27908f7b8365dd897673e21"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static EdgeEventBuffer CreateEdgeEventBuffer(int capacity = 10)
    {
        var handle = LibGpiodProxyBase.CallLibgpiod(() => LibgpiodV2.gpiod_edge_event_buffer_new(capacity));

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new edge event buffer: {LastErr.GetMsg()}");
        }

        return new EdgeEventBuffer(handle, capacity);
    }

    /// <summary>
    /// Creates a edge-event-buffer-proxy object. This call will create a new gpiod edge-event-buffer object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__edge__event.html#ga4249b081f27908f7b8365dd897673e21"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static LineConfig CreateLineConfig()
    {
        var handle = LibGpiodProxyBase.CallLibgpiod(LibgpiodV2.gpiod_line_config_new);

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new line config: {LastErr.GetMsg()}");
        }

        return new LineConfig(handle);
    }

    /// <summary>
    /// Creates a line-settings-proxy object. This call will create a new gpiod line-settings object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gab02d1cceffbb24dc95edc851e8519f0b"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static LineSettings CreateLineSettings()
    {
        var handle = LibGpiodProxyBase.CallLibgpiod(LibgpiodV2.gpiod_line_settings_new);

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new line settings: {LastErr.GetMsg()}");
        }

        return new LineSettings(handle);
    }

    /// <summary>
    /// Creates a a request-config-proxy object. This call will create a new gpiod request-config object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#gaca72f4c114efce4aa3909a3f71fb6c8e"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public static RequestConfig CreateRequestConfig()
    {
        var handle = LibGpiodProxyBase.CallLibgpiod(LibgpiodV2.gpiod_request_config_new);

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new request config: {LastErr.GetMsg()}");
        }

        return new RequestConfig(handle);
    }
}
