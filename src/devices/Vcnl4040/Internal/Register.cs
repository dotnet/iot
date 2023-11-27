// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Representation of a device register with a size of 2 bytes.
    /// </summary>
    internal abstract class Register
    {
        private readonly I2cDevice _i2cDevice;
        private readonly CommandCode _commandCode;

        protected static byte AlterIfChanged(bool changedFlag, byte dataByte, byte property, byte mask)
        {
            if (changedFlag)
            {
                dataByte = (byte)(dataByte & ~mask | property);
            }

            return dataByte;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Register"/> class.
        /// </summary>
        /// <param name="commandCode">Associated command code / address</param>
        /// <param name="device">I2C device instance for reading/writing register from/to device</param>
        protected Register(CommandCode commandCode, I2cDevice device)
        {
            _commandCode = commandCode;
            _i2cDevice = device;
        }

        /// <summary>
        /// Reads the register data from the device.
        /// </summary>
        public abstract void Read();

        /// <summary>
        /// Writes the register data to the device.
        /// </summary>
        public abstract void Write();

        /// <summary>
        /// Performs a read operation on a 16-bit register and returns low and high byte.
        /// </summary>
        protected (byte DataLow, byte DataHigh) ReadData()
        {
            Span<byte> writeBuffer = stackalloc byte[1];
            Span<byte> readBuffer = stackalloc byte[2];
            writeBuffer[0] = (byte)_commandCode;
            _i2cDevice.WriteRead(writeBuffer, readBuffer);
            return (readBuffer[0], readBuffer[1]);
        }

        /// <summary>
        /// Writes 16-bit data (split into low byte and high byte) to the register targeted by specified address.
        /// </summary>
        protected void WriteData(byte lsb, byte msb)
        {
            Span<byte> writeBuffer = stackalloc byte[3];
            writeBuffer[0] = (byte)_commandCode;
            writeBuffer[1] = lsb;
            writeBuffer[2] = msb;
            _i2cDevice.Write(writeBuffer);
        }
    }
}
