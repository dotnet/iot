// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    public class UnixI2cDevice : II2cDevice
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

        private unsafe void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public I2cConnectionSettings GetConnectionSettings()
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public ushort ReadUInt16()
        {
            throw new NotImplementedException();
        }

        public uint ReadUInt32()
        {
            throw new NotImplementedException();
        }

        public ulong ReadUInt64()
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public void WriteUInt16(ushort value)
        {
            throw new NotImplementedException();
        }

        public void WriteUInt32(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteUInt64(ulong value)
        {
            throw new NotImplementedException();
        }
    }
}
