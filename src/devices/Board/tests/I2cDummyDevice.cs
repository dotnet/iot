// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Text;

namespace Board.Tests
{
    internal class I2cDummyDevice : I2cDevice
    {
        private bool _disposed;
        public I2cDummyDevice(I2cConnectionSettings settings)
        {
            ConnectionSettings = settings;
            _disposed = false;
        }

        public override I2cConnectionSettings ConnectionSettings { get; }

        public override byte ReadByte()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This dummy instance is disposed");
            }

            if (ConnectionSettings.DeviceAddress != 55)
            {
                throw new IOException("Nothing at this address");
            }

            return 0xFF;
        }

        public override void Read(Span<byte> buffer)
        {
            throw new IOException("No answer from device");
        }

        public override void WriteByte(byte value)
        {
            throw new IOException("No answer from device");
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new IOException("No answer from device");
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new IOException("No answer from device");
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
