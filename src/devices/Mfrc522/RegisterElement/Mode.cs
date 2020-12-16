// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum Mode
    {
        MSBFirst = 0b1000_0000,
        TxWaitRF = 0b0010_0000,
        PolMFinHigh = 0b0000_1000,
    }

    internal enum ModeCrc
    {
        Preset0000h = 0,
        Preset6363h = 1,
        PresetA671h = 2,
        PresetFFFFh = 3,
    }
}
