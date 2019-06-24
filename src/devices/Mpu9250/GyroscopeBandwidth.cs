// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// Gyroscope frequency used for measurement
    /// </summary>
    public enum GyroscopeBandwidth
    {
        Bandwidth0250Hz = 0,
        Bandwidth0184Hz = 1,
        Bandwidth0092Hz = 2,
        Bandwidth0041Hz = 3,
        Bandwidth0020Hz = 4,
        Bandwidth0010Hz = 5,
        Bandwidth0005Hz = 6,
        Bandwidth3600Hz = 7,
        Bandwidth3600HzFS32 = -1,
        Bandwidth8800HzFS32 = -2,
    }
}
