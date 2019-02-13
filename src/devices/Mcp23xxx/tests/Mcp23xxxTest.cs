// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.Spi;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class Mcp23xxxTest
    {
        public static TheoryData<TestDevice> TestDevices
        {
            get
            {
                TheoryData<TestDevice> devices = new TheoryData<TestDevice>();

                // Don't want to use the same bus mock for each
                I2cDeviceMock i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23008(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23009(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23017(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23018(i2c), i2c.DeviceMock));

                SpiDeviceMock spi = new SpiDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23S08(0x20, spi), spi.DeviceMock));
                spi = new SpiDeviceMock(1);
                devices.Add(new TestDevice(new Mcp23S09(0x20, spi), spi.DeviceMock));
                spi = new SpiDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23S17(0x20, spi), spi.DeviceMock));
                spi = new SpiDeviceMock(2);
                devices.Add(new TestDevice(new Mcp23S18(0x20, spi), spi.DeviceMock));
                return devices;
            }
        }

        public struct TestDevice
        {
            public Mcp23xxx Device { get; }
            public Mcp23xxxMock DeviceMock { get; }

            public TestDevice(Mcp23xxx device, Mcp23xxxMock deviceMock)
            {
                Device = device;
                DeviceMock = deviceMock;
            }
        }

        protected class SpiDeviceMock : SpiDevice
        {
            public Mcp23xxxMock DeviceMock { get; private set; }

            public SpiDeviceMock(int ports)
            {
                DeviceMock = new Mcp23xxxMock(ports, isSpi: true);
            }

            public override void Read(Span<byte> buffer) => DeviceMock.Read(buffer);
            public override void Write(Span<byte> data) => DeviceMock.Write(data);

            public override void TransferFullDuplex(Span<byte> writeBuffer, Span<byte> readBuffer)
            {
                Write(writeBuffer);
                Read(readBuffer);
            }

            // Don't need these
            public override void WriteByte(byte data) => throw new NotImplementedException();
            public override byte ReadByte() => throw new NotImplementedException();
            public override SpiConnectionSettings ConnectionSettings => throw new NotImplementedException();
        }

        protected class I2cDeviceMock : I2cDevice
        {
            private I2cConnectionSettings _settings;
            public Mcp23xxxMock DeviceMock { get; private set; }

            public I2cDeviceMock(int ports, I2cConnectionSettings settings = null)
            {
                DeviceMock = new Mcp23xxxMock(ports, isSpi: false);
                _settings = settings ?? new I2cConnectionSettings(0, 0x20);
            }

            public override I2cConnectionSettings ConnectionSettings => _settings;

            public override void Read(Span<byte> buffer) => DeviceMock.Read(buffer);
            public override void Write(Span<byte> data) => DeviceMock.Write(data);

            // Don't need these
            public override void WriteByte(byte data) => throw new NotImplementedException();
            public override byte ReadByte() => throw new NotImplementedException();
        }

        public class Mcp23xxxMock
        {
            private int _ports;
            private bool _isSpi;
            // OLATB address is 0x15
            private byte[] _registers;
            private byte[] _lastReadBuffer;
            private byte[] _lastWriteBuffer;

            public Mcp23xxxMock(int ports, bool isSpi)
            {
                _ports = ports;
                _isSpi = isSpi;
                _registers = new byte[ports == 1 ? 0x0A + 1 : 0x15A + 1];
            }

            public Span<byte> Registers => _registers;

            // Can't coalesce here https://github.com/dotnet/roslyn/issues/29927
            public ReadOnlySpan<byte> LastReadBuffer => _lastReadBuffer == null ? ReadOnlySpan<byte>.Empty : _lastReadBuffer;
            public ReadOnlySpan<byte> LastWriteBuffer => _lastWriteBuffer == null ? ReadOnlySpan<byte>.Empty : _lastWriteBuffer;

            public void Read(Span<byte> buffer)
            {
                _lastReadBuffer = buffer.ToArray();
                if (_isSpi)
                    buffer = buffer.Slice(2);

                byte registerAddress = _lastWriteBuffer[0];

                // By default the Mcp chips shift registers sequentially
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = _registers[i + registerAddress];
                }
            }

            public void Write(Span<byte> data)
            {
                if (_isSpi)
                    data = data.Slice(1);
                _lastWriteBuffer = data.ToArray();

                byte registerAddress = data[0];
                data = data.Slice(1);

                // GPIO to OLAT (as GPIO writes change OLAT)
                // These are the address in default addressing mode (BANK0)
                if ((_ports == 2 && (registerAddress == 0x12 || registerAddress == 0x13))
                    || (_ports == 1 && (registerAddress == 0x09)))
                {
                    registerAddress = (byte)(registerAddress + _ports);
                }

                // By default the Mcp chips shift registers sequentially
                for (int i = 0; i < data.Length; i++)
                {
                    _registers[i + registerAddress] = data[i];
                }
            }
        }
    }
}
