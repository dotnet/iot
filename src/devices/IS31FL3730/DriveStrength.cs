// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// LED Drive Strength.
    /// </summary>
    public enum DriveStrength : byte
    {
        /// <summary>
        /// 5mA Drive Strength.
        /// </summary>
        Drive5ma = 0b00001000,

        /// <summary>
        /// 10mA Drive Strength.
        /// </summary>
        Drive10ma = 0b00001001,

        /// <summary>
        /// 15mA Drive Strength.
        /// </summary>
        Drive15ma = 0b00001010,

        /// <summary>
        /// 20mA Drive Strength.
        /// </summary>
        Drive20ma = 0b00001011,

        /// <summary>
        /// 25mA Drive Strength.
        /// </summary>
        Drive25ma = 0b00001100,

        /// <summary>
        /// 30mA Drive Strength.
        /// </summary>
        Drive30ma = 0b00001101,

        /// <summary>
        /// 35mA Drive Strength.
        /// </summary>
        Drive35ma = 0b00001110,

        /// <summary>
        /// 40mA Drive Strength.
        /// </summary>
        Drive40ma = 0b00000000,

        /// <summary>
        /// 45mA Drive Strength.
        /// </summary>
        Drive45ma = 0b00000001,

        /// <summary>
        /// 50mA Drive Strength.
        /// </summary>
        Drive50ma = 0b00000010,

        /// <summary>
        /// 55mA Drive Strength.
        /// </summary>
        Drive55ma = 0b00000011,

        /// <summary>
        /// 60mA Drive Strength.
        /// </summary>
        Drive60ma = 0b00000100,

        /// <summary>
        /// 65mA Drive Strength.
        /// </summary>
        Drive65ma = 0b00000101,

        /// <summary>
        /// 70mA Drive Strength.
        /// </summary>
        Drive70ma = 0b00000110,

        /// <summary>
        /// 75mA Drive Strength.
        /// </summary>
        Drive75ma = 0b00000111
    }
}
