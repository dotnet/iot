// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// 10 addressable heater profiles stored on the Bme680.
    /// </summary>
    public enum Bme680HeaterProfile : byte
    {
        /// <summary>
        /// Heater Profile 1.
        /// </summary>
        Profile1 = 0b0000,
        /// <summary>
        /// Heater Profile 2.
        /// </summary>
        Profile2 = 0b0001,
        /// <summary>
        /// Heater Profile 3.
        /// </summary>
        Profile3 = 0b0010,
        /// <summary>
        /// Heater Profile 4.
        /// </summary>
        Profile4 = 0b0011,
        /// <summary>
        /// Heater Profile 5.
        /// </summary>
        Profile5 = 0b0100,
        /// <summary>
        /// Heater Profile 6.
        /// </summary>
        Profile6 = 0b0101,
        /// <summary>
        /// Heater Profile 7.
        /// </summary>
        Profile7 = 0b0110,
        /// <summary>
        /// Heater Profile 8.
        /// </summary>
        Profile8 = 0b0111,
        /// <summary>
        /// Heater Profile 9.
        /// </summary>
        Profile9 = 0b1000,
        /// <summary>
        /// Heater Profile 10.
        /// </summary>
        Profile10 = 0b1001
    }
}
