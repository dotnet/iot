// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Base class for 16-bit level registers of ALS and PS.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal abstract class LevelRegister : Register
    {
        /// <summary>
        /// Gets or sets the level (threshold).
        /// </summary>
        public ushort Level { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelRegister"/> class.
        /// </summary>
        public LevelRegister(CommandCode commandCode, I2cDevice device)
            : base(commandCode, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Level = (ushort)(dataHigh << 8 | dataLow);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            WriteData((byte)Level, (byte)(Level >> 8));
        }
    }
}
