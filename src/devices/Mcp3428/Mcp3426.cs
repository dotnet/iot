// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Mcp3428
{
    /// <summary>
    /// Represents Mcp3426 ADC
    /// </summary>
    public class Mcp3426 : Mcp342x
    {
        private const int NumChannels = 2;

        /// <summary>
        /// Default I2C address for the device
        /// </summary>
        public const int I2CAddress = 0x68;

        /// <summary>
        /// Constructs Mcp3426 instance
        /// </summary>
        /// <param name="i2CDevice">I2C device used to communicate with the device</param>
        public Mcp3426(I2cDevice i2CDevice)
            : base(i2CDevice, NumChannels)
        {
        }

        /// <summary>
        /// Constructs Mcp3426 instance
        /// </summary>
        /// <param name="i2CDevice">I2C device used to communicate with the device</param>
        /// <param name="mode">ADC operation mode</param>
        /// <param name="resolution">ADC resolution</param>
        /// <param name="pgaGain">PGA gain</param>
        public Mcp3426(I2cDevice i2CDevice, AdcMode mode = AdcMode.Continuous, AdcResolution resolution = AdcResolution.Bit12, AdcGain pgaGain = AdcGain.X1)
            : this(i2CDevice) => SetConfig(0, mode: mode, resolution: resolution, pgaGain: pgaGain);
    }
}
