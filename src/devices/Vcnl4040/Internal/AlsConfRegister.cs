// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
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
        private static readonly byte AlsItMask = 0b1100_0000;
        private static readonly byte AlsPersMask = 0b0000_1100;
        private static readonly byte AlsIntEnMask = 0b0000_0010;
        private static readonly byte AlsSdMask = 0b0000_0001;

        /// <summary>
        /// ALS integration time setting
        /// </summary>
        public AlsIntegrationTime AlsIt { get; set; } = AlsIntegrationTime.Time80ms;

        /// <summary>
        /// ALS interrupt persistence setting
        /// </summary>
        public AlsInterruptPersistence ALS_PERS { get; set; } = AlsInterruptPersistence.Persistence1;

        /// <summary>
        /// ALS interrupt enable state
        /// </summary>
        public AlsInterrupt ALS_INT_EN { get; set; } = AlsInterrupt.Disabled;

        /// <summary>
        /// ALS power state (ALS_SD of ALS_CONF register)
        /// </summary>
        public PowerState AlsSd { get; set; } = PowerState.Shutdown;

        public AlsConfRegister(I2cInterface bus)
            : base(CommandCode.ALS_CONF, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte _) = ReadData();

            AlsIt = (AlsIntegrationTime)(dataLow & AlsItMask);
            ALS_PERS = (AlsInterruptPersistence)(dataLow & AlsPersMask);
            ALS_INT_EN = (AlsInterrupt)(dataLow & AlsIntEnMask);
            AlsSd = (PowerState)(dataLow & AlsSdMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            byte dataLow = 0;
            dataLow |= (byte)((byte)(AlsIt) << 6);
            dataLow |= (byte)((byte)(ALS_PERS) << 2);
            dataLow |= (byte)((byte)(ALS_INT_EN) << 1);
            dataLow |= (byte)AlsSd;

            WriteData(dataLow, 0);
        }
    }
}
