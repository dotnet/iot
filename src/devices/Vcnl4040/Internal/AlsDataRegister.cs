// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// ALS data output register
    /// Note: this is a read-only register.
    /// Command code / address: 0x09
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class AlsDataRegister : Register
    {
        public int Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlsDataRegister"/> class.
        /// </summary>
        public AlsDataRegister(I2cDevice device)
            : base(CommandCode.ALS_Data, device)
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
