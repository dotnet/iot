// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    [Flags]
    internal enum Coll
    {
        ValuesAfterColl = 0b1000_0000,
        CollPosNotValid = 0b0010_0000,
    }
}
