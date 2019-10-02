// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Magnetometer;
using System;
using System.Device;
using System.Device.I2c;

namespace Iot.Device.Imu
{
    internal class Ak8963Attached : Ak8963I2cBase
    {
        /// <summary>
        /// Read a byte
        /// </summary>
        /// <param name="i2cDevice">An I2C device</param>
        /// <param name="reg">The register to read</param>
        /// <param name="data">A byte to write</param>        
        public override void WriteRegister(I2cDevice i2cDevice, byte reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[2] { (byte)Register.I2C_SLV0_ADDR, Magnetometer.Ak8963.DefaultI2cAddress };
            i2cDevice.Write(dataout);
            dataout[0] = (byte)Register.I2C_SLV0_REG;
            dataout[1] = reg;
            i2cDevice.Write(dataout);
            dataout[0] = (byte)Register.I2C_SLV0_DO;
            dataout[1] = data;
            i2cDevice.Write(dataout);
            dataout[0] = (byte)Register.I2C_SLV0_CTRL;
            dataout[1] = 0x81;
            i2cDevice.Write(dataout);
        }

        /// <summary>
        /// Read a byte array
        /// </summary>
        /// <param name="i2cDevice">An I2C device</param>
        /// <param name="reg">>The register to read</param>
        /// <returns>The register value</returns>
        public override byte ReadByte(I2cDevice i2cDevice, byte reg)
        {
            Span<byte> read = stackalloc byte[1] { 0 };
            ReadBytes(i2cDevice, reg, read);
            return read[0];
        }

        /// <summary>
        /// Write a byte
        /// </summary>
        /// <param name="i2cDevice">>An I2C device</param>
        /// <param name="reg">The register to read</param>
        /// <param name="readBytes">A span of bytes with the read values</param>
        public override void ReadBytes(I2cDevice i2cDevice, byte reg, Span<byte> readBytes)
        {
            Span<byte> dataout = stackalloc byte[2] { (byte)Register.I2C_SLV0_ADDR, Magnetometer.Ak8963.DefaultI2cAddress | 0x80 };
            i2cDevice.Write(dataout);
            dataout[0] = (byte)Register.I2C_SLV0_REG;
            dataout[1] = reg;
            i2cDevice.Write(dataout);
            dataout[0] = (byte)Register.I2C_SLV0_CTRL;
            dataout[1] = (byte)(0x80 | readBytes.Length);
            i2cDevice.Write(dataout);
            // TODO: delay found empirically, spec does not mention delay but it is observable it is required
            DelayHelper.DelayMicroseconds(200, false);
            i2cDevice.WriteByte((byte)Register.EXT_SENS_DATA_00);
            i2cDevice.Read(readBytes);
        }
    }
}
