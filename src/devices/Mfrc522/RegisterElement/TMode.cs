// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum TMode
    {
        TAuto = 0b1000_0000,
        TGatedNonGated = 0b0000_0000,
        TGatedPinMFIN = 0b0010_0000,
        TGatedPnAUX1 = 0b0100_0000,
        TAutoRestart = 0b0001_0000,
    }
}
