// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Iot.Device.Vncl4040.Infrastructure
{
    /// <summary>
    /// Device register interface for registers with a width of 1 or 2 bytes.
    /// </summary>
    public abstract class Register
    {
        /// <summary>
        /// I2C bus instance to be used for reading/writing register from/to device.
        /// </summary>
        private readonly I2cInterface _bus;

        private readonly byte _address;

        /// <summary>
        /// Initializes the base class for any derived register with up to 3 addresses.
        /// </summary>
        /// <param name="address">First register address</param>
        /// <param name="bus">I2C bus interface</param>
        protected Register(byte address, I2cInterface bus)
        {
            _address = address;
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
        /// Performs a read operation.
        /// </summary>
        protected (byte Lsb, byte Msb) ReadData()
        {
            return (_bus.Read(_address), _bus.Read(_address);
        }

        /// <summary>
        /// Writes 'data' to the specified address, first applying a mask to the current content
        /// of the address before the write access is performed.
        /// </summary>
        protected void WriteData(byte lsb, byte msb)
        {
            WriteInternal(lsb, 0xff, msb, 0xff);
        }

        /// <summary>
        /// Writes 'data' to the specified address, first applying a mask to the current content
        /// of the address before the write access is performed.
        /// Mask: only bits with '1' are modified. All other bits a preserved.
        /// </summary>
        protected void WriteDataPreserve(byte data, byte mask) => WriteInternal(data, mask);

        /// <summary>
        /// Writes 'data' to the specified address, first applying a mask to the current content
        /// of the address before the write access is performed.
        /// Mask: only bits with '1' are modified. All other bits a preserved.
        /// </summary>
        protected void WriteDataPreserve(byte dataA, byte maskA,
                                         byte dataB, byte maskB)
            => WriteInternal(dataA, maskA, dataB, maskB);

        private void WriteInternal(byte lsb, byte maskA = 0xff,
                                   byte msb, byte maskB = 0xff)
        {
            if (maskA != null)
            {
                byte regData = _bus.Read(_addressA);

                // set bits to be modified to 0
                regData &= (byte)~maskA.Value;
                regData |= (byte)(dataA & maskA.Value);
                _bus.Write(_addressA, regData);
            }
            else
            {
                _bus.Write(_addressA, dataA);
            }

            // Intentionally leave here if B is not valid, even if C would be (which would be weired anyway)
            if (dataB == null || _addressB == Address.NoAddress)
            {
                return;
            }

            if (maskB != null)
            {
                byte regData = _bus.Read(_addressB);
                regData &= (byte)~maskB.Value;
                regData |= (byte)(dataB.Value & maskB.Value);
                _bus.Write(_addressB, regData);
            }
            else
            {
                _bus.Write(_addressB, dataB.Value);
            }

            if (dataC == null || _addressC == Address.NoAddress)
            {
                return;
            }

            if (maskC != null)
            {
                byte regData = _bus.Read(_addressC);
                regData &= (byte)~maskC.Value;
                regData |= (byte)(dataC.Value & maskC.Value);
                _bus.Write(_addressC, regData);
            }
            else
            {
                _bus.Write(_addressC, dataC.Value);
            }
        }
    }
}
