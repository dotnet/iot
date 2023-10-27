// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS configuration register 1 & 2
    /// Command code / address: 0x03 (LSB and MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf12Register : Register
    {
        private static readonly byte PsDutyMask = 0b1100_0000;
        private static readonly byte PsPersMask = 0b0011_0000;
        private static readonly byte PsItMask = 0b0000_1110;
        private static readonly byte PsSdMask = 0b0000_0001;
        private static readonly byte PsHdMask = 0b0001_0000;
        private static readonly byte PsIntMask = 0b0000_0011;

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
        /// PS output size
        /// </summary>
        public PsOutput PsHd { get; set; } = PsOutput.Bits12;

        /// <summary>
        /// PS interrupt source
        /// </summary>
        public PsInterrupt PsInt { get; set; } = PsInterrupt.Disable;

        public PsConf12Register(I2cInterface bus)
            : base(CommandCode.PS_CONF_1_2, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            PsDuty = (PsDuty)(byte)(dataLow & PsDutyMask);
            PsPers = (PsInterruptPersistence)(byte)(dataLow & PsPersMask);
            PsIt = (PsIntegrationTime)(byte)(dataLow & PsItMask);
            PsSd = (PowerState)(byte)(dataLow & PsSdMask);
            PsHd = (PsOutput)(byte)(dataHigh & PsHdMask);
            PsInt = (PsInterrupt)(byte)(dataHigh & PsIntMask);
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            byte dataLow = 0;
            dataLow |= (byte)PsDuty;
            dataLow |= (byte)PsPers;
            dataLow |= (byte)PsIt;
            dataLow |= (byte)PsSd;

            byte dataHigh = 0;
            dataHigh |= (byte)PsHd;
            dataHigh |= (byte)PsInt;

            WriteData(dataLow, dataHigh);
        }
    }
}
