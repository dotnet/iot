// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Bno055
{
    [Flags]
    public enum InteruptStatus
    {
        GyroscopeInterupt = 0b0000_0100,
        GyroscopeHighRateInterupt = 0b0000_1000,
        AccelerometerHighRateInterupt = 0b0010_0000,
        AccelerometerAnyMotionInterupt = 0b0100_0000,
        AccelerometerNoMotionInterup = 0b1000_0000,
    }
}
