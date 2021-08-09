// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// I2C Device for FT232H
    /// </summary>
    public class Ft232HI2c : I2cDevice
    {
        private Ft232HI2cBus _i2cBus;
        private int _deviceAddress;
        private I2cConnectionSettings _settings;

        internal Ft232HI2c(Ft232HI2cBus i2cBus, int deviceAddress)
        {
            _i2cBus = i2cBus;
            _deviceAddress = deviceAddress;
            _settings = new I2cConnectionSettings((int)i2cBus.DeviceInformation.LocId, deviceAddress);
        }

        /// <inheritdoc/>
        public override I2cConnectionSettings ConnectionSettings => _settings;

        /// <inheritdoc/>
        public override void Read(Span<byte> buffer)
        {
            _i2cBus.Read(_deviceAddress, buffer);
        }

        /// <inheritdoc/>
        public override byte ReadByte()
        {
            Span<byte> toRead = stackalloc byte[1];
            Read(toRead);
            return toRead[0];
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _i2cBus.Write(_deviceAddress, buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            Span<byte> toWrite = stackalloc byte[1]
            {
                value
            };
            Write(toWrite);
        }

        /// <inheritdoc/>
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            _i2cBus.Write(_deviceAddress, writeBuffer);
            _i2cBus.Read(_deviceAddress, readBuffer);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _i2cBus?.RemoveDevice(_deviceAddress);
            _i2cBus = null!;
            _settings = null!;
            base.Dispose(disposing);
        }

    }
}
