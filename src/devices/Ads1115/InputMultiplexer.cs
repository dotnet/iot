// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Configure the Input Multiplexer
    /// </summary>
    public enum InputMultiplexer
    {
        // Details in Datasheet P12 Figure24

        /// <summary>
        ///  AIN Positive = AIN0 and AIN Negative  = GND
        ///  Measure the Voltage between AIN0 and GND
        /// </summary>
        AIN0 = 0x04,

        /// <summary>
        /// AIN Positive = AIN1 and AIN Negative = GND
        /// Measure the Voltage between AIN1 and GND
        /// </summary>
        AIN1 = 0x05,

        /// <summary>
        /// AIN Positive = AIN2 and AIN Negative = GND
        /// Measure the Voltage between AIN2 and GND
        /// </summary>
        AIN2 = 0x06,

        /// <summary>
        /// AIN Positive = AIN3 and AIN Negative = GND
        /// Measure the Voltage between AIN3 and GND
        /// </summary>
        AIN3 = 0x07,

        /// <summary>
        /// AIN Positive = AIN0 and AIN Negative = AIN1
        /// Measure the Voltage between AIN0 and AIN1
        /// </summary>
        AIN0_AIN1 = 0x00,

        /// <summary>
        /// AIN Positive = AIN0 and AIN Negative = AIN3
        /// Measure the Voltage between AIN0 and AIN3
        /// </summary>
        AIN0_AIN3 = 0x01,

        /// <summary>
        /// AIN Positive = AIN1 and AIN Negative = AIN3
        /// Measure the Voltage between AIN1 and AIN3
        /// </summary>
        AIN1_AIN3 = 0x02,

        /// <summary>
        /// AIN Positive = AIN2 and AIN Negative = AIN3
        /// Measure the Voltage between AIN2 and AIN3
        /// </summary>
        AIN2_AIN3 = 0x03,
    }
}
