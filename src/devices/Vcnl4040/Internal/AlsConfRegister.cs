// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

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
        private const byte ReservedBitsMask = 0b0011_0000;

        /// <summary>
        /// ALS integration time setting
        /// </summary>
        public AlsIntegrationTime AlsIt { get; set; } = AlsIntegrationTime.Time80ms;

        /// <summary>
        /// ALS interrupt persistence setting
        /// </summary>
        public AlsInterruptPersistence AlsPers { get; set; } = AlsInterruptPersistence.Persistence1;

        /// <summary>
        /// ALS interrupt enable state
        /// </summary>
        public AlsInterrupt AlsIntEn { get; set; } = AlsInterrupt.Disabled;

        /// <summary>
        /// ALS power state (ALS_SD of ALS_CONF register)
        /// </summary>
        public PowerState AlsSd { get; set; } = PowerState.PowerOff;

        public AlsConfRegister(I2cInterface bus)
            : base(CommandCode.ALS_CONF, bus)
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
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            dataLow &= ReservedBitsMask;
            dataLow |= (byte)AlsIt;
            dataLow |= (byte)AlsPers;
            dataLow |= (byte)AlsIntEn;
            dataLow |= (byte)AlsSd;

            WriteData(dataLow, dataHigh);
        }
    }
}
