// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// ID register
    /// Note: this is a read-only register.
    /// Command code / address: 0x0c (LSB, MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class IdRegister : Register
    {
        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdRegister"/> class.
        /// </summary>
        public IdRegister(I2cDevice device)
            : base(CommandCode.ID, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Id = (dataHigh << 8 | dataLow) & 0b0000_1111_1111_1111;
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
