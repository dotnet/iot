// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 2
    /// Command code / address: 0x03 (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf2Register : Register
    {
        private static readonly byte PsHdMask = 0b0001_0000;
        private static readonly byte PsIntMask = 0b0000_0011;

        /// <summary>
        /// PS output size
        /// </summary>
        public PsOutput PsHd { get; set; } = PsOutput.Bits12;

        /// <summary>
        /// PS interrupt source
        /// </summary>
        public PsInterrupt PsInt { get; set; } = PsInterrupt.Disable;

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf2Register"/> class.
        /// </summary>
        public PsConf2Register(I2cInterface bus)
            : base(CommandCode.PS_CONF_1_2, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (_, byte dataHigh) = ReadData();

            PsHd = (PsOutput)(dataHigh & PsHdMask);
            PsInt = (PsInterrupt)(dataHigh & PsIntMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content to preserve low byte
            (byte dataLow, _) = ReadData();

            byte dataHigh = 0;
            dataHigh |= (byte)PsHd;
            dataHigh |= (byte)PsInt;

            WriteData(dataLow, dataHigh);
        }
    }
}
