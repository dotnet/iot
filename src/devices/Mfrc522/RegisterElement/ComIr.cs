// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum ComIr
    {
        TimerIRq = 0b0000_0001,
        ErrIRq = 0b0000_0010,
        LoAlertIRq = 0b0000_0100,
        HiAlertIRq = 0b0000_1000,
        IdleIRq = 0b0001_0000,
        RxIRq = 0b0010_0000,
        TxIRq = 0b0100_0000,
        SetIrq = 0b1000_0000,
        All = TimerIRq | ErrIRq | LoAlertIRq | HiAlertIRq | IdleIRq | RxIRq | TxIRq,
        None = 0b0000_0000
    }
}
