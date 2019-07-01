// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1750fvi
{
    internal enum Command : byte
    {
        PowerDown = 0b_0000_0000,
        PowerOn = 0b_0000_0001,
        Reset = 0b_0000_0111,
        MeasurementTimeHigh = 0b_0100_0000,
        MeasurementTimeLow = 0b_0110_0000,
    }
}