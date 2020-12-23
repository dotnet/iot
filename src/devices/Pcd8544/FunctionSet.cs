// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Display.Pcd8544Enums
{
    [Flags]
    internal enum FunctionSet
    {
        PowerOn = 0b0010_0000,
        PowerOff = 0b0010_0100,
        ExtendedMode = 0b0010_0001,
        HorizontalAddressing = 0b0010_0000,
        VerticalAddressing = 0b0010_0010,
    }
}
