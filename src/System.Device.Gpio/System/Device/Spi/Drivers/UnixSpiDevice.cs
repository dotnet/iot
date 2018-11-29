// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi.Devices
{
    public class UnixSpiDevice : SpiDevice
    {
        private const string _defaultDevicePath = "/dev/spidev";
        private SpiConnectionSettings _settings;

        private UnixSpiDevice() { }

        public UnixSpiDevice(SpiConnectionSettings settings)
        {
            _settings = settings;
            DevicePath = _defaultDevicePath;
        }

        public string DevicePath { get; set; }

        public override SpiConnectionSettings ConnectionSettings => throw new NotImplementedException();

        private unsafe void Initialize()
        {
            throw new NotImplementedException();
        }

        public override byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte data)
        {
            throw new NotImplementedException();
        }

        public override void Write(Span<byte> data)
        {
            throw new NotImplementedException();
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
