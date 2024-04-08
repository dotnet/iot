// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 1
    /// Command code / address: 0x03 (LSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf1Register : Register
    {
        private const byte PsDutyMask = 0b1100_0000;
        private const byte PsPersMask = 0b0011_0000;
        private const byte PsItMask = 0b0000_1110;
        private const byte PsSdMask = 0b0000_0001;
        private bool _pPsDutyChanged = false;
        private bool _psPersChanged = false;
        private bool _psItChanged = false;
        private bool _psSdChanged = false;
        private PsDuty _psDuty = PsDuty.Duty40;
        private PsInterruptPersistence _psPers = PsInterruptPersistence.Persistence1;
        private PsIntegrationTime _psIt = PsIntegrationTime.Time1_0;
        private PowerState _psSd = PowerState.PowerOff;

        /// <summary>
        /// Gets or sets the duty ratio (PS_Duty).
        /// </summary>
        public PsDuty PsDuty
        {
            get => _psDuty;
            set
            {
                _psDuty = value;
                _pPsDutyChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the interrupt persistence (PS_PERS).
        /// </summary>
        public PsInterruptPersistence PsPers
        {
            get => _psPers;
            set
            {
                _psPers = value;
                _psPersChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the integration time (PS_IT).
        /// </summary>
        public PsIntegrationTime PsIt
        {
            get => _psIt;
            set
            {
                _psIt = value;
                _psItChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the power state (PS_SD).
        /// </summary>
        public PowerState PsSd
        {
            get => _psSd;
            set
            {
                _psSd = value;
                _psSdChanged = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf1Register"/> class.
        /// </summary>
        public PsConf1Register(I2cDevice device)
            : base(CommandCode.PS_CONF_1_2, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, _) = ReadData();

            PsDuty = (PsDuty)(dataLow & PsDutyMask);
            PsPers = (PsInterruptPersistence)(dataLow & PsPersMask);
            PsIt = (PsIntegrationTime)(dataLow & PsItMask);
            PsSd = (PowerState)(dataLow & PsSdMask);

            ResetChangeFlags();
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content, to preserve the high byte
            (byte dataLow, byte dataHigh) = ReadData();

            dataLow = AlterIfChanged(_pPsDutyChanged, dataLow, (byte)PsDuty, PsDutyMask);
            dataLow = AlterIfChanged(_psPersChanged, dataLow, (byte)PsPers, PsPersMask);
            dataLow = AlterIfChanged(_psItChanged, dataLow, (byte)PsIt, PsItMask);
            dataLow = AlterIfChanged(_psSdChanged, dataLow, (byte)PsSd, PsSdMask);

            WriteData(dataLow, dataHigh);
            ResetChangeFlags();
        }

        private void ResetChangeFlags()
        {
            _pPsDutyChanged = false;
            _psPersChanged = false;
            _psItChanged = false;
            _psSdChanged = false;
        }
    }
}
