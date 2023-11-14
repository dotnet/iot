// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Base class for various interrupt threshold registers
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal abstract class InterruptThresholdRegister : Register
    {
        /// <summary>
        /// Interrupt threshold setting.
        /// </summary>
        public int Threshold { get; set; } = 0;

        public InterruptThresholdRegister(CommandCode commandCode, I2cDevice device)
            : base(commandCode, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Threshold = dataHigh << 8 | dataLow;
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            WriteData((byte)(Threshold & 0xff), (byte)((Threshold >> 8) & 0xff));
        }
    }
}
