// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS MS register
    /// Command code / address: 0x04 (LSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf3Register : Register
    {
        private const byte PsMpsMask = 0b0110_0000;
        private const byte PsSmartPersMask = 0b0001_0000;
        private const byte PsAfMask = 0b0000_1000;
        private const byte PsTrigMask = 0b0000_0100;
        private const byte PsScEnMask = 0b0000_0001;
        private bool _psMpsChanged = false;
        private bool _psSmartPersChanged = false;
        private bool _psAfChanged = false;
        private bool _psTrigChanged = false;
        private bool _psScEnChanged = false;
        private PsMultiPulse _psMps = PsMultiPulse.Pulse1;
        private PsSmartPersistenceState _psSmartPers = PsSmartPersistenceState.Disabled;
        private PsActiveForceMode _psAf = PsActiveForceMode.Disabled;
        private PsActiveForceModeTrigger _psTrig = PsActiveForceModeTrigger.NoTrigger;
        private PsSunlightCancellationState _psScEn = PsSunlightCancellationState.Disabled;

        /// <summary>
        /// Gets or sets the multi pulse setting (PS_MPS).
        /// </summary>
        public PsMultiPulse PsMps
        {
            get => _psMps;
            set
            {
                _psMps = value;
                _psMpsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the smart persistence state (PS_SMART_PERS).
        /// </summary>
        public PsSmartPersistenceState PsSmartPers
        {
            get => _psSmartPers;
            set
            {
                _psSmartPers = value;
                _psSmartPersChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the active force mode state (PS_AF).
        /// </summary>
        public PsActiveForceMode PsAf
        {
            get => _psAf;
            set
            {
                _psAf = value;
                _psAfChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the active force mode trigger (PS_TRIG).
        /// </summary>
        public PsActiveForceModeTrigger PsTrig
        {
            get => _psTrig;
            set
            {
                _psTrig = value;
                _psTrigChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the sunlight cancellation state (PS_SC_EN).
        /// </summary>
        public PsSunlightCancellationState PsScEn
        {
            get => _psScEn;
            set
            {
                _psScEn = value;
                _psScEnChanged = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf3Register"/> class.
        /// </summary>
        public PsConf3Register(I2cDevice device)
            : base(CommandCode.PS_CONF_3_MS, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, _) = ReadData();

            PsMps = (PsMultiPulse)(dataLow & PsMpsMask);
            PsSmartPers = (PsSmartPersistenceState)(dataLow & PsSmartPersMask);
            PsAf = (PsActiveForceMode)(dataLow & PsAfMask);
            PsTrig = (PsActiveForceModeTrigger)(dataLow & PsTrigMask);
            PsScEn = (PsSunlightCancellationState)(dataLow & PsScEnMask);

            ResetChangeFlags();
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            dataLow = AlterIfChanged(_psMpsChanged, dataLow, (byte)PsMps, PsMpsMask);
            dataLow = AlterIfChanged(_psSmartPersChanged, dataLow, (byte)PsSmartPers, PsSmartPersMask);
            dataLow = AlterIfChanged(_psAfChanged, dataLow, (byte)PsAf, PsAfMask);
            dataLow = AlterIfChanged(_psTrigChanged, dataLow, (byte)PsTrig, PsTrigMask);
            dataLow = AlterIfChanged(_psScEnChanged, dataLow, (byte)PsScEn, PsScEnMask);

            WriteData(dataLow, dataHigh);
            ResetChangeFlags();
        }

        private void ResetChangeFlags()
        {
            _psMpsChanged = false;
            _psSmartPersChanged = false;
            _psAfChanged = false;
            _psTrigChanged = false;
            _psScEnChanged = false;
        }
    }
}
