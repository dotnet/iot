// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 2
    /// Command code / address: 0x03 (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf2Register : Register
    {
        private const byte PsHdMask = 0b0000_1000;
        private const byte PsIntMask = 0b0000_0011;
        private bool _psHdChanged = false;
        private bool _psIntChanged = false;
        private PsOutputRange _psHd = PsOutputRange.Bits12;
        private PsInterruptMode _psInt = PsInterruptMode.Disabled;

        /// <summary>
        /// Gets or sets the output range (PS_HD).
        /// </summary>
        public PsOutputRange PsHd
        {
            get => _psHd;
            set
            {
                _psHd = value;
                _psHdChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the interrupt event source (PS_INT).
        /// </summary>
        public PsInterruptMode PsInt
        {
            get => _psInt;
            set
            {
                _psInt = value;
                _psIntChanged = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf2Register"/> class.
        /// </summary>
        public PsConf2Register(I2cDevice device)
            : base(CommandCode.PS_CONF_1_2, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (_, byte dataHigh) = ReadData();

            PsHd = (PsOutputRange)(dataHigh & PsHdMask);
            PsInt = (PsInterruptMode)(dataHigh & PsIntMask);

            ResetChangeFlags();
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            dataHigh = AlterIfChanged(_psHdChanged, dataHigh, (byte)PsHd, PsHdMask);
            dataHigh = AlterIfChanged(_psIntChanged, dataHigh, (byte)PsInt, PsIntMask);

            WriteData(dataLow, dataHigh);
            ResetChangeFlags();
        }

        private void ResetChangeFlags()
        {
            _psHdChanged = false;
            _psIntChanged = false;
        }
    }
}
