// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp3428
{
    public class Mcp3426 : Mcp3428
    {
        public const int Address = 0x68;

        /// <inheritdoc />
        public Mcp3426(I2cDevice i2CDevice) : base(i2CDevice, 2)
        {
        }

        /// <inheritdoc />
        public Mcp3426(I2cDevice i2CDevice, ModeEnum mode = ModeEnum.Continuous, ResolutionEnum resolution = ResolutionEnum.Bit12, GainEnum pgaGain = GainEnum.X1) : this(i2CDevice)
        {
            SetConfig(0, resolution: resolution, mode: mode, pgaGain: pgaGain);
        }

        public new static int AddressFromPins(PinState _ = PinState.Low, PinState _NA = PinState.Low) { return Address; }
    }
}
