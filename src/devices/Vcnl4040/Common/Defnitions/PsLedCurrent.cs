// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the set of PS LED currents.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsLedCurrent : byte
    {
        /// <summary>
        /// LED current 50 mA
        /// </summary>
        I50mA = 0b0000_0000,

        /// <summary>
        /// LED current 75 mA
        /// </summary>
        I75mA = 0b0000_0001,

        /// <summary>
        /// LED current 100 mA
        /// </summary>
        I100mA = 0b0000_0010,

        /// <summary>
        /// LED current 120 mA
        /// </summary>
        I120mA = 0b0000_0011,

        /// <summary>
        /// LED current 140 mA
        /// </summary>
        I140mA = 0b0000_0100,

        /// <summary>
        /// LED current 160 mA
        /// </summary>
        I160mA = 0b0000_0101,

        /// <summary>
        /// LED current 180 mA
        /// </summary>
        I180mA = 0b0000_0110,

        /// <summary>
        /// LED current 200 mA
        /// </summary>
        I200mA = 0b0000_0111
    }
}
