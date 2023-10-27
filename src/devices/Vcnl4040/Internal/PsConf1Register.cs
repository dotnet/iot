// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 1
    /// Command code / address: 0x03 (LSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf1Register : Register
    {
        private static readonly byte PsDutyMask = 0b1100_0000;
        private static readonly byte PsPersMask = 0b0011_0000;
        private static readonly byte PsItMask = 0b0000_1110;
        private static readonly byte PsSdMask = 0b0000_0001;

        /// <summary>
        /// PS IRED on/off duty ratio
        /// </summary>
        public PsDuty PsDuty { get; set; } = PsDuty.Duty80;

        /// <summary>
        /// PS interrupt persistence
        /// </summary>
        public PsInterruptPersistence PsPers { get; set; } = PsInterruptPersistence.Persistence1;

        /// <summary>
        /// PS integration time
        /// </summary>
        public PsIntegrationTime PsIt { get; set; } = PsIntegrationTime.Time1_0;

        /// <summary>
        /// PS power state
        /// </summary>
        public PowerState PsSd { get; set; } = PowerState.Shutdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf1Register"/> class.
        /// </summary>
        public PsConf1Register(I2cInterface bus)
            : base(CommandCode.PS_CONF_1_2, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            PsDuty = (PsDuty)(dataLow & PsDutyMask);
            PsPers = (PsInterruptPersistence)(dataLow & PsPersMask);
            PsIt = (PsIntegrationTime)(dataLow & PsItMask);
            PsSd = (PowerState)(dataLow & PsSdMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content, to preserve the high byte
            (_, byte dataHigh) = ReadData();

            byte dataLow = 0;
            dataLow |= (byte)PsDuty;
            dataLow |= (byte)PsPers;
            dataLow |= (byte)PsIt;
            dataLow |= (byte)PsSd;

            WriteData(dataLow, dataHigh);
        }
    }
}
