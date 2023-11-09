// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 3
    /// Command code / address: 0x04 (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsMsRegister : Register
    {
        private const byte WhiteEnMask = 0b1000_0000;
        private const byte PsMsMask = 0b0100_0000;
        private const byte LedIMask = 0b0000_0111;

        /// <summary>
        /// PS white channel state
        /// </summary>
        public PsWhiteChannelState WhiteEn { get; set; } = PsWhiteChannelState.Enabled;

        /// <summary>
        /// PS detection logic output mode
        /// </summary>
        public PsDetectionLogicOutputMode PsMs { get; set; } = PsDetectionLogicOutputMode.Interrupt;

        /// <summary>
        /// PS LED current
        /// </summary>
        public PsLedCurrent LedI { get; set; } = PsLedCurrent.I50mA;

        /// <summary>
        /// Initializes a new instance of the <see cref="PsMsRegister"/> class.
        /// </summary>
        public PsMsRegister(I2cInterface bus)
            : base(CommandCode.PS_CONF_3_MS, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (_, byte dataHigh) = ReadData();

            WhiteEn = (PsWhiteChannelState)(dataHigh & WhiteEnMask);
            PsMs = (PsDetectionLogicOutputMode)(dataHigh & PsMsMask);
            LedI = (PsLedCurrent)(dataHigh & LedIMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content to preserve low byte
            (byte dataLow, _) = ReadData();

            byte dataHigh = 0;
            dataHigh |= (byte)WhiteEn;
            dataHigh |= (byte)PsMs;
            dataHigh |= (byte)LedI;

            WriteData(dataLow, dataHigh);
        }
    }
}
