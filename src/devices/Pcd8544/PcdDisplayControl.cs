// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display.Pcd8544Enums
{
    internal enum PcdDisplayControl
    {
        DisplayBlank = 0b0000_1000,
        NormalMode = 0b0000_1100,
        AllSegmentsOn = 0b0000_1001,
        InverseVideoMode = 0b0000_1101,
    }
}
