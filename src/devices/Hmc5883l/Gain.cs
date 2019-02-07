// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// HMC5883L Gain Setting
    /// </summary>
    public enum Gain
    {
        /// <summary>
        /// 1370, recommended sensor field range: ±0.88 Ga
        /// </summary>
        Gain1 = 0x00,

        /// <summary>
        /// 1090, recommended sensor field range: ±1.3 Ga
        /// </summary>
        Gain2 = 0x01,

        /// <summary>
        /// 820, recommended sensor field range: ±1.9 Ga
        /// </summary>
        Gain3 = 0x02,

        /// <summary>
        /// 660, recommended sensor field range: ±2.5 Ga
        /// </summary>
        Gain4 = 0x03,

        /// <summary>
        /// 440, recommended sensor field range: ±4.0 Ga
        /// </summary>
        Gain5 = 0x04,

        /// <summary>
        /// 390, recommended sensor field range: ±4.7 Ga
        /// </summary>
        Gain6 = 0x05,

        /// <summary>
        /// 330, recommended sensor field range: ±5.6 Ga
        /// </summary>
        Gain7 = 0x06,

        /// <summary>
        /// 230, recommended sensor field range: ±8.1 Ga
        /// </summary>
        Gain8 = 0x07,
    }
}
