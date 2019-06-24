// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// Frequency of the slave I2C bus
    /// </summary>
    public enum I2cBussFrequency
    {
        Freqency348kHz = 0,
        Freqency333kHz = 1,
        Freqency320kHz = 2,
        Freqency308kHz = 3,
        Freqency296kHz = 4,
        Freqency286kHz = 5,
        Freqency276kHz = 6,
        Freqency267kHz = 7,
        Freqency258kHz = 8,
        Freqency500kHz = 9,
        Freqency471kHz = 10,
        Freqency444kHz = 11,
        Freqency421kHz = 12,
        Freqency400kHz = 13,
        Freqency381kHz = 14,
        Freqency364kHz = 15,
    }
}
