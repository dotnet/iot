// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// 10 addressable heater profiles saved on the Bme680.
    /// </summary>
    public enum HeaterProfile : byte
    {
        /// <summary>
        /// Heating Profile 1.
        /// </summary>
        Profile1 = 0b0000,
        /// <summary>
        /// Heating Profile 2.
        /// </summary>
        Profile2 = 0b0001,
        /// <summary>
        /// Heating Profile 3.
        /// </summary>
        Profile3 = 0b0010,
        /// <summary>
        /// Heating Profile 4.
        /// </summary>
        Profile4 = 0b0011,
        /// <summary>
        /// Heating Profile 5.
        /// </summary>
        Profile5 = 0b0100,
        /// <summary>
        /// Heating Profile 6.
        /// </summary>
        Profile6 = 0b0101,
        /// <summary>
        /// Heating Profile 7.
        /// </summary>
        Profile7 = 0b0110,
        /// <summary>
        /// Heating Profile 8.
        /// </summary>
        Profile8 = 0b0111,
        /// <summary>
        /// Heating Profile 9.
        /// </summary>
        Profile9 = 0b1000,
        /// <summary>
        /// Heating Profile 10.
        /// </summary>
        Profile10 = 0b1001
    }
}
