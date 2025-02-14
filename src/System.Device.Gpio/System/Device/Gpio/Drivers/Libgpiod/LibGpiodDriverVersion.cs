// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Each driver version supports specific libgpiod version/s.
/// </summary>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
public enum LibGpiodDriverVersion
{
    /// <summary>
    /// Version 1 of the libgpiod driver. Supports libgpiod v0 to v1 (libgpiod.so.1.x to libgpiod.so.2.x)
    /// </summary>
    V1,

    /// <summary>
    /// Version 2 of the libgpiod driver. Supports libgpiod v2 (libgpiod.so.3.x)
    /// </summary>
    V2
}
