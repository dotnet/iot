// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Exception in the context of calling libgpiod
/// </summary>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
public class GpiodException : IOException
{
    /// <summary>
    /// Exception in the context of calling libgpiod
    /// </summary>
    public GpiodException()
    {
    }

    /// <summary>
    /// Exception in the context of calling libgpiod
    /// </summary>
    public GpiodException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Exception in the context of calling libgpiod
    /// </summary>
    public GpiodException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
