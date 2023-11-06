// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Infrastructure
{
    /// <summary>
    /// Device register interface for registers with a width of 1 or 2 bytes.
    /// </summary>
    internal abstract class Register
    {
        /// <summary>
        /// I2C bus instance to be used for reading/writing register from/to device.
        /// </summary>
        private readonly I2cInterface _bus;

        private readonly CommandCode _commandCode;

        /// <summary>
        /// Initializes a new instance of Register.
        /// </summary>
        /// <param name="commandCode">Register address / command code</param>
        /// <param name="bus">I2C bus interface</param>
        protected Register(CommandCode commandCode, I2cInterface bus)
        {
            _commandCode = commandCode;
            _bus = bus;
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
        /// Performs a read operation on a 16-bit register.
        /// </summary>
        protected (byte DataLow, byte DataHigh) ReadData()
        {
            byte[] data = new byte[2];
            _bus.Read(_commandCode, data);
            return (data[0], data[1]);
        }

        /// <summary>
        /// Writes 16-bit data (split into low byte and high byte) to the register targeted by specified address.
        /// </summary>
        protected void WriteData(byte dataLow, byte dataHigh)
        {
            WriteInternal(dataLow, 0xff, dataHigh, 0xff);
        }

        /// <summary>
        /// Writes 16-bit data (split into low byte and high byte) to the register targeted by specified command code.
        /// It preserves existing register content as selected by the masks.
        /// Only mask bits set to '1' are modified. All other bits are preserved.
        /// </summary>
        protected void WriteDataPreserve(byte dataLow, byte maskLow, byte dataHigh, byte maskHigh) => WriteInternal(dataLow, maskLow, dataHigh, maskHigh);

        private void WriteInternal(byte dataLow, byte maskLow, byte dataHigh, byte maskHigh)
        {
            (byte DataLow, byte DataHigh) regData = (0, 0);
            if (maskLow != 0xff || maskHigh != 0xff)
            {
                regData = ReadData();
            }

            if (maskLow != 0xff)
            {
                // set bits to be modified to 0
                regData.DataLow &= (byte)~maskLow;
                regData.DataLow |= (byte)(dataLow & maskLow);
            }
            else
            {
                regData.DataLow = dataLow;
            }

            if (maskHigh != 0xff)
            {
                regData.DataHigh &= (byte)~maskHigh;
                regData.DataHigh |= (byte)(dataHigh & maskHigh);
            }
            else
            {
                regData.DataHigh = dataHigh;
            }

            byte[] data = new byte[2];
            data[0] = regData.DataLow;
            data[1] = regData.DataHigh;
            _bus.Write(_commandCode, data);
        }
    }
}
