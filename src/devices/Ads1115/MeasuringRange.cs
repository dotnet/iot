// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Configure the Programmable Gain Amplifier, i.e. Measuring Range
	/// Note that the maximum input value on any input pin is VDD+0.3V and the maximum value that can be measured is VDD. 
	/// So if the supply voltage is 3.3V, using FS6144 may not be useful, because it just reduces the accuracy to 14 bit
	/// (excluding the sign bit).
    /// </summary>
    public enum MeasuringRange
    {
        // Details in Datasheet P19

        /// <summary>
        /// ±6.144V.
        /// </summary>
        FS6144 = 0x00,

        /// <summary>
        /// ±4.096V
        /// </summary>
        FS4096 = 0x01,

        /// <summary>
        /// ±2.048V
        /// </summary>
        FS2048 = 0x02,

        /// <summary>
        /// ±1.024V
        /// </summary>
        FS1024 = 0x03,

        /// <summary>
        /// ±0.512V
        /// </summary>
        FS0512 = 0x04,

        /// <summary>
        /// ±0.256V
        /// </summary>
        FS0256 = 0x05
    }
}
