// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Different device families supported by the driver.
    /// </summary>
    public enum DeviceFamily : int
    {
        /// <summary>
        /// Special device family used when enumerating all devices on a bus.
        /// </summary>
        Any = -1,
        /// <summary>
        /// Family id of a DS18S20 temperature sensor.
        /// </summary>
        Ds18s20 = 0x10,
        /// <summary>
        /// Family id of a DS18B20 temperature sensor.
        /// </summary>
        Ds18b20 = 0x28,
        /// <summary>
        /// Family id of a MAX31820 temperature sensor.
        /// </summary>
        Max31820 = 0x28,
        /// <summary>
        /// Family id of a DS1825 temperature sensor.
        /// </summary>
        Ds1825 = 0x3B,
        /// <summary>
        /// Family id of a MAX31826 temperature sensor.
        /// </summary>
        Max31826 = 0x3B,
        /// <summary>
        /// Family id of a MAX31850 temperature sensor.
        /// </summary>
        Max31850 = 0x3B,
        /// <summary>
        /// Family id of a DS28EA00 temperature sensor.
        /// </summary>
        Ds28ea00 = 0x42,
        /// <summary>
        /// Special family id used to enumerate all temperature sensors.
        /// </summary>
        Thermometer = 0x100,
    }
}
