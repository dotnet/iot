// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// ID register
    /// Note: this is a read-only register.
    /// Command code / address: 0x0c
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class IdRegister : Register
    {
        /// <summary>
        /// LSB byte of device Id.
        /// There is no further description in the datasheet.
        /// </summary>
        public byte IdLsb { get; private set; }

        /// <summary>
        /// Slave address of the device.
        /// This is 0 for the default address 60h.
        /// There is no further description in the datasheet.
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        /// Version code of the device. This is usually 0b0001.
        /// There is no further description in the datasheet.
        /// </summary>
        public byte VersionCode { get; private set; }

        /// <summary>
        /// Gets the complete device Id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdRegister"/> class.
        /// </summary>
        public IdRegister(I2cInterface bus)
            : base(CommandCode.ID, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();

            IdLsb = dataLow;
            SlaveAddress = (byte)(dataHigh & 0b0011_0000 >> 4);
            VersionCode = (byte)(dataHigh & 0b0000_1111);
            Id = dataHigh << 8 | dataLow;
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
