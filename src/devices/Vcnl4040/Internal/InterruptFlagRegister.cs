// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Vcnl4040.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Interrupt flag register
    /// Note: this is a read-only register.
    /// Command code / address: 0x0b
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class InterruptFlagRegister : Register
    {
        /// <summary>
        /// PS entering protection mode event
        /// </summary>
        public bool PsSpFlag { get; private set; }

        /// <summary>
        /// ALS crossing low threshold event
        /// </summary>
        public bool AlsIfL { get; private set; }

        /// <summary>
        /// ALS crossing high threshold event
        /// </summary>
        public bool AlsIfH { get; private set; }

        /// <summary>
        /// PS rises above threshold event
        /// </summary>
        public bool PsIfClose { get; private set; }

        /// <summary>
        /// PS drops below threshold event
        /// </summary>
        public bool PsIfAway { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterruptFlagRegister"/> class.
        /// </summary>
        public InterruptFlagRegister(I2cInterface bus)
            : base(CommandCode.INT_Flag, bus)
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
        /// This register is read-only. The Write-operation is not implemented.
        /// </summary>
        /// <exception cref="System.NotImplementedException">This register is read-only. The Write-operation is not implemented.</exception>
        public override void Write()
        {
            // Note: this is a read-only register
            throw new System.NotImplementedException();
        }
    }
}
