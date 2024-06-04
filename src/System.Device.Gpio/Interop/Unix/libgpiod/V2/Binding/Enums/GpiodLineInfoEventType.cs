// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html"/>
internal enum GpiodLineInfoEventType
{
    LineRequested = 1,
    LineReleased = 2,
    LineConfigChanged = 3
}
