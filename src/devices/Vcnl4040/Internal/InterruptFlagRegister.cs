// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Interrupt flag register
    /// Note: this is a read-only register.
    /// Command code / address: 0x0b (MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class InterruptFlagRegister : Register
    {
        /// <summary>
        /// Gets the PS entering protection mode interrupt event has been triggered.
        /// </summary>
        public bool PsSpFlag { get; private set; }

        /// <summary>
        /// Gets whether the ALS crossing low threshold interrupt event has been triggered.
        /// </summary>
        public bool AlsIfL { get; private set; }

        /// <summary>
        /// Gets whether the ALS crossing high threshold interrupt event has been triggered.
        /// </summary>
        public bool AlsIfH { get; private set; }

        /// <summary>
        /// Gets whether the PS rises above threshold interrupt event has been triggered.
        /// </summary>
        public bool PsIfClose { get; private set; }

        /// <summary>
        /// Gets whether the PS drops below threshold interrupt event has been triggered.
        /// </summary>
        public bool PsIfAway { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterruptFlagRegister"/> class.
        /// Note: reading the registers resets the set flags.
        /// </summary>
        public InterruptFlagRegister(I2cDevice device)
            : base(CommandCode.INT_Flag, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (_, byte dataHigh) = ReadData();

            PsSpFlag = (dataHigh & 0b0100_0000) > 0;
            AlsIfL = (dataHigh & 0b0010_0000) > 0;
            AlsIfH = (dataHigh & 0b0001_0000) > 0;
            PsIfClose = (dataHigh & 0b0000_0010) > 0;
            PsIfAway = (dataHigh & 0b0000_0001) > 0;
        }

        /// <summary>
        /// This register is read-only. The write-operation is not implemented.
        /// </summary>
        /// <exception cref="System.NotImplementedException">This register is read-only. The write-operation is not implemented.</exception>
        public override void Write()
        {
            throw new System.NotImplementedException();
        }
    }
}
