// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vncl4040.Definitions;
using Iot.Device.Vncl4040.Infrastructure;

namespace Iot.Device.Vncl4040.Internal
{
    /// <summary>
    /// ALS configuration register
    /// Address / command: 0x00
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class AlsConfRegister : Register
    {
        /// <summary>
        /// ALS integration time setting
        /// </summary>
        public AlsIntegrationTime ALS_IT { get; set; } = AlsIntegrationTime.IntegrationTime80ms;

        /// <summary>
        /// ALS interrupt persistence setting
        /// </summary>
        public AlsInterruptPersistence ALS_PERS { get; set; } = AlsInterruptPersistence.Persistence1;

        /// <summary>
        /// ALS interrupt enable state
        /// </summary>
        public AlsInterruptState ALS_INT_EN { get; set; } = AlsInterruptState.Disabled;

        /// <summary>
        /// ALS power state
        /// </summary>
        public AlsPowerState ALS_SD { get; set; } = AlsPowerState.Shutdown;

        public AlsConfRegister(I2cInterface bus)
            : base(Address.ALS_CONF, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte _) = ReadData();

            ALS_IT = (AlsIntegrationTime)(byte)((dataLow & 0b1100_0000) >> 6);
            ALS_PERS = (AlsInterruptPersistence)(byte)((dataLow & 0b0000_1100) >> 2);
            ALS_INT_EN = (AlsInterruptState)(byte)((dataLow & 0b0000_0010) >> 1);
            ALS_SD = (AlsPowerState)(byte)(dataLow & 0b0000_0001);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            byte dataLow = 0;
            dataLow |= (byte)((byte)(ALS_IT) << 6);
            dataLow |= (byte)((byte)(ALS_PERS) << 2);
            dataLow |= (byte)((byte)(ALS_INT_EN) << 1);
            dataLow |= (byte)ALS_SD;

            WriteData(dataLow, 0);
        }
    }
}
