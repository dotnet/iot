// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Default I2C interface for the AK8963
    /// </summary>
    public class Ak8963I2c : Ak8963I2cBase
    {
        /// <summary>
        /// Read a byte
        /// </summary>
        /// <param name="i2cDevice">An I2C device</param>
        /// <param name="reg">The register to read</param>
        /// <returns>The register value</returns>
        public override byte ReadByte(I2cDevice i2cDevice, byte reg)
        {
            i2cDevice.WriteByte(reg);
            return i2cDevice.ReadByte();
        }

        /// <summary>
        /// Read a byte array
        /// </summary>
        /// <param name="i2cDevice">An I2C device</param>
        /// <param name="reg">>The register to read</param>
        /// <param name="readBytes">A span of bytes with the read values</param>
        public override void ReadBytes(I2cDevice i2cDevice, byte reg, Span<byte> readBytes)
        {
            i2cDevice.WriteByte(reg);
            i2cDevice.Read(readBytes);
        }

        /// <summary>
        /// Write a byte
        /// </summary>
        /// <param name="i2cDevice">>An I2C device</param>
        /// <param name="reg">The register to read</param>
        /// <param name="data">A byte to write</param>
        public override void WriteRegister(I2cDevice i2cDevice, byte reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[]
            {
                reg,
                data
            };
            i2cDevice.Write(dataout);
        }
    }
}
