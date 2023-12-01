// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace System.Device.Gpio.Interop.Unix.libgpiod.V2.Binding.Enums;

/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html"/>
internal enum GpiodLineInfoEventType
{
    LineRequested = 1,
    LineReleased,
    LineConfigChanged
}
