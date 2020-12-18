// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum TxControl
    {
        InvTx2RFOn = 0b1000_0000,
        InvTx1RFOn = 0b0100_0000,
        InvTx2RFOff = 0b0010_0000,
        InvTx1RFOff = 0b0001_0000,
        Tx2CW = 0b0000_1000,
        Tx2RFEn = 0b0000_0010,
        Tx1RFEn = 0b0000_0001,
    }
}
