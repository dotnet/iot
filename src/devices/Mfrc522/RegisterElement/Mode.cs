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
}
