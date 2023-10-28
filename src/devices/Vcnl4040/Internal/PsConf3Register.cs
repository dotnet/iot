// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS MS register
    /// Command code / address: 0x04 (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsConf3Register : Register
    {
        private static readonly byte PsMpsMask = 0b0110_0000;
        private static readonly byte PsSmartPersMask = 0b0001_0000;
        private static readonly byte PsAfMask = 0b0000_1000;
        private static readonly byte PsTrigMask = 0b0000_0100;
        private static readonly byte PsScEnMask = 0b0000_0001;

        /// <summary>
        /// PS multi pulse setting
        /// </summary>
        public PsMultiPulse PsMps { get; set; } = PsMultiPulse.Pulse1;

        /// <summary>
        /// PS smart persistence state
        /// </summary>
        public PsSmartPersistenceState PsSmartPers { get; set; } = PsSmartPersistenceState.Disabled;

        /// <summary>
        /// PS active force mode
        /// </summary>
        public PsActiveForceMode PsAf { get; set; } = PsActiveForceMode.Disabled;

        /// <summary>
        /// PS active force mode trigger
        /// </summary>
        public PsActiveForceModeTrigger PsTrig { get; set; } = PsActiveForceModeTrigger.NoTrigger;

        /// <summary>
        /// PS sunlight cancellation state
        /// </summary>
        public PsSunlightCancellationState PsScEn { get; set; } = PsSunlightCancellationState.Disabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="PsConf3Register"/> class.
        /// </summary>
        public PsConf3Register(I2cInterface bus)
            : base(CommandCode.PS_CONF_3_MS, bus)
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
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            // read current register content to preserve high byte
            (_, byte dataHigh) = ReadData();

            byte dataLow = 0;
            dataLow |= (byte)PsMps;
            dataLow |= (byte)PsSmartPers;
            dataLow |= (byte)PsAf;
            dataLow |= (byte)PsTrig;
            dataLow |= (byte)PsScEn;

            WriteData(dataLow, dataHigh);
        }
    }
}
