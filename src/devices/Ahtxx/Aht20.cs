// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Ahtxx
{
    /// <summary>
    /// AHT20 temperature and humidity sensor binding.
    /// </summary>
    public class Aht20 : AhtBase
    {
        /// <summary>
        /// Initialization command acc. to datasheet
        /// </summary>
        private const byte Aht20InitCommand = 0b1011_1110;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aht20"/> class.
        /// </summary>
        public Aht20(I2cDevice i2cDevice)
            : base(i2cDevice, Aht20InitCommand)
        {
        }
    }
}
