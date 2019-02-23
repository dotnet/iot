// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Bno055
{
    [Flags]
    public enum Units
    {
        AccelerationMeterPerSecond = 0b0000_0000,
        AccelerationMeterG = 0b0000_0001,
        AngularRateDegreePerSecond = 0b0000_0000,
        AngularRateRotationPerSecond = 0b0000_0010,
        EulerAnglesDegrees = 0b0000_0000,
        EulerAnglesRadians = 0b0000_0100,
        TemperatureCelsius = 0b0000_0000,
        TemperatureFarenheit = 0b0001_0000,
        DataOutputFormatWindows = 0b0000_0000,
        DataOutputFormatAndroid = 0b1000_0000,
    }
}
