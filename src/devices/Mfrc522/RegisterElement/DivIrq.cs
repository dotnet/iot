// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum DivIrq
    {
        CRCIRq = 0b0000_0100,
        MfinActIRq = 0b0001_0000,
        Set2 = 0b1000_0000
    }
}
