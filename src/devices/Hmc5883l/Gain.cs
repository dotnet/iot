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
        Gain1370 = 0x00,

        /// <summary>
        /// 1090, recommended sensor field range: ±1.3 Ga
        /// </summary>
        Gain1090 = 0x01,

        /// <summary>
        /// 820, recommended sensor field range: ±1.9 Ga
        /// </summary>
        Gain0820 = 0x02,

        /// <summary>
        /// 660, recommended sensor field range: ±2.5 Ga
        /// </summary>
        Gain0660 = 0x03,

        /// <summary>
        /// 440, recommended sensor field range: ±4.0 Ga
        /// </summary>
        Gain0440 = 0x04,

        /// <summary>
        /// 390, recommended sensor field range: ±4.7 Ga
        /// </summary>
        Gain0390 = 0x05,

        /// <summary>
        /// 330, recommended sensor field range: ±5.6 Ga
        /// </summary>
        Gain0330 = 0x06,

        /// <summary>
        /// 230, recommended sensor field range: ±8.1 Ga
        /// </summary>
        Gain0230 = 0x07,
    }
}
