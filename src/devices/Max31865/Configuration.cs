// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Max31865
{
    [Flags]
    internal enum Configuration : byte
    {
        Filter60HZ = 0b_0000_0000,

        Filter50HZ = 0b_0000_0001,

        FaultStatus = 0b_0000_0010,

        TwoFourWire = 0b_0000_0000,

        ThreeWire = 0b_0001_0000,

        OneShot = 0b_0010_0000,

        ConversionModeAuto = 0b_0100_0000,

        Bias = 0b_1000_0000
    }
}
