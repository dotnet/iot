// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

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
