// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80.Register
{
    /// <summary>
    /// General control registers for the BME280.
    /// </summary>
    internal enum Bme280Register : byte
    {
        CTRL_HUM = 0xF2,

        DIG_H1 = 0xA1,
        DIG_H2 = 0xE1,
        DIG_H3 = 0xE3,
        DIG_H4 = 0xE4,
        DIG_H5 = 0xE5,
        DIG_H6 = 0xE7,

        HUMIDDATA = 0xFD
    }
}
