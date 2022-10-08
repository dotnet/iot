// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;

namespace Iot.Device.Lis3DhAccelerometer
{
    internal sealed class Lis3DhI2c : Lis3Dh
    {
        private I2cDevice _i2c;

        public Lis3DhI2c(I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }

        private protected override void ReadRegister(Register register, Span<byte> data, bool autoIncrement = true)
        {
            Span<byte> reg = stackalloc byte[1];
            reg[0] = GetRegister(register, autoIncrement);
            _i2c.WriteRead(reg, data);
        }

        private protected override void WriteRegister(Register register, byte data, bool autoIncrement = true)
        {
            Span<byte> reg = stackalloc byte[2];
            reg[0] = GetRegister(register, autoIncrement);
            reg[1] = data;
            _i2c.Write(reg);
        }

        private static byte GetRegister(Register register, bool autoIncrement)
            => (byte)((byte)register | (autoIncrement ? (byte)Register.I2cAutoIncrement : (byte)0));
    }
}
