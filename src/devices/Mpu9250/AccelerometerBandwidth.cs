// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// Bandwith used for normal measurement of the acceleromter
    /// using filter block. This can be further reduced using 
    /// SampleRateDivider with all modes except 1130Hz.
    /// </summary>
    public enum AccelerometerBandwidth
    {
        Bandwidth1130Hz = 0,
        Bandwidth0460Hz = 0x08,
        Bandwidth0184Hz = 0x09,
        Bandwidth0092Hz = 0x0A,
        Bandwidth0041Hz = 0x0B,
        Bandwidth0020Hz = 0x0C,
        Bandwidth0010Hz = 0x0E,
        Bandwidth0005Hz = 0x0F,
    }
}
