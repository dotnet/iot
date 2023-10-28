// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Vcnl4040.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// White data output register
    /// Note: this is a read-only register.
    /// Command code / address: 0x0a
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class WhiteDataRegister : Register
    {
        public int Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhiteDataRegister"/> class.
        /// </summary>
        public WhiteDataRegister(I2cInterface bus)
            : base(CommandCode.White_Data, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Data = dataHigh << 8 | dataLow;
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
