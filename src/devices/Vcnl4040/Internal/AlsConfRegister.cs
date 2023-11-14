// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// ALS configuration register
    /// Command code / address: 0x00
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class AlsConfRegister : Register
    {
        private const byte AlsItMask = 0b1100_0000;
        private const byte AlsPersMask = 0b0000_1100;
        private const byte AlsIntEnMask = 0b0000_0010;
        private const byte AlsSdMask = 0b0000_0001;
        private bool _alsItChanged = false;
        private bool _alsPersChanged = false;
        private bool _alsIntEnChanged = false;
        private bool _alsSdChanged = false;
        private PowerState _alsSd = PowerState.PowerOff;
        private AlsInterrupt _alsIntEn = AlsInterrupt.Disabled;
        private AlsInterruptPersistence _alsPers = AlsInterruptPersistence.Persistence1;
        private AlsIntegrationTime _alsIt = AlsIntegrationTime.Time80ms;

        /// <summary>
        /// ALS integration time setting
        /// </summary>
        public AlsIntegrationTime AlsIt
        {
            get => _alsIt;
            set
            {
                _alsIt = value;
                _alsItChanged = true;
            }
        }

        /// <summary>
        /// ALS interrupt persistence setting
        /// </summary>
        public AlsInterruptPersistence AlsPers
        {
            get => _alsPers;
            set
            {
                _alsPers = value;
                _alsPersChanged = true;
            }
        }

        /// <summary>
        /// ALS interrupt enable state
        /// </summary>
        public AlsInterrupt AlsIntEn
        {
            get => _alsIntEn;
            set
            {
                _alsIntEn = value;
                _alsIntEnChanged = true;
            }
        }

        /// <summary>
        /// ALS power state (ALS_SD of ALS_CONF register)
        /// </summary>
        public PowerState AlsSd
        {
            get => _alsSd;
            set
            {
                _alsSd = value;
                _alsSdChanged = true;
            }
        }

        public AlsConfRegister(I2cDevice device)
            : base(CommandCode.ALS_CONF, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte _) = ReadData();

            AlsIt = (AlsIntegrationTime)(dataLow & AlsItMask);
            AlsPers = (AlsInterruptPersistence)(dataLow & AlsPersMask);
            AlsIntEn = (AlsInterrupt)(dataLow & AlsIntEnMask);
            AlsSd = (PowerState)(dataLow & AlsSdMask);
            ResetChangeFlags();
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // Read current LSB and MSB, to preserve MSB and reserved bits from LSB.
            (byte dataLow, byte dataHigh) = ReadData();
            dataLow = AlterIfChanged(_alsItChanged, dataLow, (byte)AlsIt, AlsItMask);
            dataLow = AlterIfChanged(_alsPersChanged, dataLow, (byte)AlsPers, AlsPersMask);
            dataLow = AlterIfChanged(_alsIntEnChanged, dataLow, (byte)AlsIntEn, AlsIntEnMask);
            dataLow = AlterIfChanged(_alsSdChanged, dataLow, (byte)AlsSd, AlsSdMask);

            WriteData(dataLow, dataHigh);
            ResetChangeFlags();
        }

        private void ResetChangeFlags()
        {
            _alsItChanged = false;
            _alsPersChanged = false;
            _alsIntEnChanged = false;
            _alsSdChanged = false;
        }
    }
}
