// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ads1115
{
    /// <summary>
    ///  Configure the Input Multiplexer
    /// </summary>
    public enum InputMultiplexeConfig
    {
        /// <summary>
        ///  AINP = AIN0 and AINN = GND 
        /// </summary>
        AIN0 = 0x04,
        /// <summary>
        /// AINP = AIN1 and AINN = GND
        /// </summary>
        AIN1 = 0x05,
        /// <summary>
        /// AINP = AIN2 and AINN = GND
        /// </summary>
        AIN2 = 0x06,
        /// <summary>
        /// AINP = AIN3 and AINN = GND
        /// </summary>
        AIN3 = 0x07,
        /// <summary>
        /// AINP = AIN0 and AINN = AIN1
        /// Measure the Voltage between AINP and AINN
        /// </summary>
        AIN0_AIN1 = 0x00,
        /// <summary>
        /// AINP = AIN0 and AINN = AIN3
        /// Measure the Voltage between AINP and AINN
        /// </summary>
        AIN0_AIN3 = 0x00,
        /// <summary>
        /// AINP = AIN1 and AINN = AIN3
        /// Measure the Voltage between AINP and AINN
        /// </summary>
        AIN1_AIN3 = 0x00,
        /// <summary>
        /// AINP = AIN2 and AINN = AIN3
        /// Measure the Voltage between AINP and AINN
        /// </summary>
        AIN2_AIN3 = 0x00,
    }
}
