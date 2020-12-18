// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum BitFraming
    {
        StartSend = 0b0000_0000,
        RxAlignLSBPosition0 = 0b0000_0000,
        RxAlignLSBPosition1 = 0b0001_0000,
        RxAlignLSBPosition7 = 0b0111_0000,
        TxLastBitsMask = 0b0000_0111,
        TxLastBitsDefault = 0b0000_0000,
    }
}
