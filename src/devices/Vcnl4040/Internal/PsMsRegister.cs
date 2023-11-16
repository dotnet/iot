// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS MS configuration register
    /// Command code / address: 0x04 (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsMsRegister : Register
    {
        private const byte WhiteEnMask = 0b1000_0000;
        private const byte PsMsMask = 0b0100_0000;
        private const byte LedIMask = 0b0000_0111;
        private bool _whiteEnChanged = false;
        private bool _psMsChanged = false;
        private bool _ledIChanged = false;
        private PsLedCurrent _ledI = PsLedCurrent.I50mA;
        private PsProximityDetectionOutput _psMs = PsProximityDetectionOutput.Interrupt;
        private PsWhiteChannelState _whiteEn = PsWhiteChannelState.Enabled;

        /// <summary>
        /// Gets or sets the white channel state (White_EN).
        /// </summary>
        public PsWhiteChannelState WhiteEn
        {
            get => _whiteEn;
            set
            {
                _whiteEn = value;
                _whiteEnChanged = true;
            }
        }

        /// <summary>
        /// Gets or set the detection logic output mode state (PS_MS).
        /// </summary>
        public PsProximityDetectionOutput PsMs
        {
            get => _psMs;
            set
            {
                _psMs = value;
                _psMsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the LED current (LED_I).
        /// </summary>
        public PsLedCurrent LedI
        {
            get => _ledI;
            set
            {
                _ledI = value;
                _ledIChanged = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PsMsRegister"/> class.
        /// </summary>
        public PsMsRegister(I2cDevice device)
            : base(CommandCode.PS_CONF_3_MS, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (_, byte dataHigh) = ReadData();

            WhiteEn = (PsWhiteChannelState)(dataHigh & WhiteEnMask);
            PsMs = (PsProximityDetectionOutput)(dataHigh & PsMsMask);
            LedI = (PsLedCurrent)(dataHigh & LedIMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content to preserve low byte
            (byte dataLow, byte dataHigh) = ReadData();

            dataHigh = AlterIfChanged(_whiteEnChanged, dataHigh, (byte)WhiteEn, WhiteEnMask);
            dataHigh = AlterIfChanged(_psMsChanged, dataHigh, (byte)PsMs, PsMsMask);
            dataHigh = AlterIfChanged(_ledIChanged, dataHigh, (byte)LedI, LedIMask);

            WriteData(dataLow, dataHigh);
            ResetChangeFlags();
        }

        private void ResetChangeFlags()
        {
            _whiteEnChanged = false;
            _psMsChanged = false;
            _ledIChanged = false;
        }
    }
}
