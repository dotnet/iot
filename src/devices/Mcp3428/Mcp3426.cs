// Mcp3428::Mcp3428
// Filename: Mcp3426.cs
// Created: __
// Edited: 20190405
// Creator: Máté Kullai

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

        public new static int AddressFromPins(PinState _, PinState _NA) { return Address; }
    }
}
