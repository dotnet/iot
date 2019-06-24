// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Ak8963
{
    /// <summary>
    /// Default I2C interface for the AK8963
    /// </summary>
    public class Ak8963I2c : Ak8963Interface
    {
        public override byte ReadByte(I2cDevice i2cDevice, Register reg)
        {
            i2cDevice.WriteByte((byte)reg);
            return i2cDevice.ReadByte();
        }

        public override void ReadByteArray(I2cDevice i2cDevice, Register reg, Span<byte> readBytes)
        {
            i2cDevice.WriteByte((byte)reg);
            i2cDevice.Read(readBytes);
        }

        public override void WriteRegister(I2cDevice i2cDevice, Register reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[] { (byte)reg, data };
            i2cDevice.Write(dataout);
        }
    }
}
