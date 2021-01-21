// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum Error
    {
        WrErr = 0b1000_0000,
        TempErr = 0b0100_0000,
        BufferOvfl = 0b0001_0000,
        CollErr = 0b0000_1000,
        CRCErr = 0b0000_0100,
        ParityErr = 0b0000_0010,
        ProtocolErr = 0b0000_0001
    }
}
