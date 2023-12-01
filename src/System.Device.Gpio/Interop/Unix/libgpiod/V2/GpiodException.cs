// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace System.Device.Gpio.Interop.Unix.libgpiod.v2;

internal class GpiodException : Exception
{
    public GpiodException()
    {
    }

    public GpiodException(string message)
        : base(message)
    {
    }

    public GpiodException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
