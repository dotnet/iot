// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    public class UnixI2cDevice : I2cDevice
    {
        private I2cConnectionSettings _settings;
        private const string _defaultDevicePath = "/dev/i2c";

        private UnixI2cDevice() { }

        public UnixI2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = _defaultDevicePath;
        }

        public string DevicePath { get; set; }

        public override I2cConnectionSettings ConnectionSettings => throw new NotImplementedException();

        private unsafe void Initialize()
        {
            throw new NotImplementedException();
        }

        public override byte ReadByte(int address)
        {
            throw new NotImplementedException();
        }

        public override void Read(int address, Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(int address, byte data)
        {
            throw new NotImplementedException();
        }

        public override void Write(int address, Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
