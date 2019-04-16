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
            SetConfig(0, mode: mode, resolution: resolution, pgaGain: pgaGain);
        }

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
        public new static int I2CAddressFromPins(PinState _ = PinState.Low, PinState _NA = PinState.Low) { return Address; }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
    }
}
