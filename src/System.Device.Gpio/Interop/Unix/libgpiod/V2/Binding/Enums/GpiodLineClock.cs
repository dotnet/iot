// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__defs.html"/>
internal enum GpiodLineClock
{
    Monotonic = 1,
    Realtime = 2,
    Hte = 3
}
