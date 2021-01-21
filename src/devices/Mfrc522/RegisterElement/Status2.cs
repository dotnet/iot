// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum Status2
    {
        TempSensClear = 0b1000_0000,
        I2CForceHS = 0b0100_0000,
        MFCrypto1On = 0b0000_1000
    }

    internal enum ModemState
    {
        Idle = 0b0000_0000,
        WaitForBitFramingReg = 0b0000_0001,
        TxWait = 0b0000_0010,
        Transmitting = 0b0000_0011,
        RxWait = 0b0000_0100,
        WaitForData = 0b0000_0101,
        Receiving = 0b0000_0110,
    }
}
