// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.I2c
{
    internal class UnixI2cDevice : I2cDevice
    {
        private UnixI2cBus _bus;
        private int _deviceAddress;
        private bool _shouldDisposeBus;

        public UnixI2cDevice(UnixI2cBus bus, int deviceAddress, bool shouldDisposeBus = false)
        {
            _bus = bus;
            _deviceAddress = deviceAddress;
            _shouldDisposeBus = shouldDisposeBus;
        }

        public override I2cConnectionSettings ConnectionSettings => new I2cConnectionSettings(_bus.BusId, _deviceAddress);

        public override unsafe byte ReadByte()
        {
            Span<byte> toRead = stackalloc byte[1];
            Read(toRead);
            return toRead[0];
        }

        public override unsafe void Read(Span<byte> buffer)
        {
            _bus.Read(_deviceAddress, buffer);
        }

        public override unsafe void WriteByte(byte value)
        {
            Span<byte> toWrite = stackalloc byte[1]
            {
                value
            };
            Write(toWrite);
        }

        public override unsafe void Write(ReadOnlySpan<byte> buffer)
        {
            _bus.Write(_deviceAddress, buffer);
        }

        public override unsafe void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            _bus.WriteRead(_deviceAddress, writeBuffer, readBuffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (_bus != null)
            {
                _bus.RemoveDeviceNoCheck(_deviceAddress);

                if (_shouldDisposeBus)
                {
                    _bus.Dispose();
                }

                _bus = null!;
            }

            base.Dispose(disposing);
        }
    }
}
