// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Pcx857x.Tests
{
    public class Pcx857xTest
    {
        public static TheoryData<TestDevice> TestDevices
        {
            get
            {
                TheoryData<TestDevice> devices = new TheoryData<TestDevice>();

                // Don't want to use the same bus mock for each
                I2cDeviceMock i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Pcf8574(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(1);
                devices.Add(new TestDevice(new Pca8574(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Pcf8575(i2c), i2c.DeviceMock));
                i2c = new I2cDeviceMock(2);
                devices.Add(new TestDevice(new Pca8575(i2c), i2c.DeviceMock));
                return devices;
            }
        }

        public struct TestDevice
        {
            public Pcx857x Device { get; }
            public Pcx857xChipMock ChipMock { get; }

            public TestDevice(Pcx857x device, Pcx857xChipMock chipMock)
            {
                Device = device;
                ChipMock = chipMock;
            }
        }

        protected class I2cDeviceMock : I2cDevice
        {
            private I2cConnectionSettings _settings;
            public Pcx857xChipMock DeviceMock { get; private set; }

            public I2cDeviceMock(int ports, I2cConnectionSettings settings = null)
            {
                DeviceMock = new Pcx857xChipMock(ports);
                _settings = settings ?? new I2cConnectionSettings(0, 0x20);
            }

            public override I2cConnectionSettings ConnectionSettings => _settings;

            public override void Read(Span<byte> buffer) => DeviceMock.Read(buffer);
            public override void Write(ReadOnlySpan<byte> data) => DeviceMock.Write(data);

            // Don't need these
            public override void WriteByte(byte data) => DeviceMock.WriteByte(data);
            public override byte ReadByte() => DeviceMock.ReadByte();
            public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) => throw new NotImplementedException();
        }

        /// <summary>
        /// Mock the behavior of the chip
        /// </summary>
        public class Pcx857xChipMock
        {
            private int _ports;
            private byte[] _registers;
            private byte[] _lastReadBuffer;
            private byte[] _lastWriteBuffer;

            public Pcx857xChipMock(int ports)
            {
                _ports = ports;
                _registers = new byte[ports];
            }

            public Span<byte> Registers => _registers;

            // Can't coalesce here https://github.com/dotnet/roslyn/issues/29927
            public ReadOnlySpan<byte> LastReadBuffer => _lastReadBuffer == null ? ReadOnlySpan<byte>.Empty : _lastReadBuffer;
            public ReadOnlySpan<byte> LastWriteBuffer => _lastWriteBuffer == null ? ReadOnlySpan<byte>.Empty : _lastWriteBuffer;

            public void Read(Span<byte> buffer)
            {
                _lastReadBuffer = buffer.ToArray();

                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = _registers[i];
                }
            }

            public void Write(ReadOnlySpan<byte> data)
            {
                _lastWriteBuffer = data.ToArray();

                for (int i = 0; i < data.Length; i++)
                {
                    _registers[i] = data[i];
                }
            }

            public byte ReadByte()
            {
                Span<byte> buffer = stackalloc byte[1];
                Read(buffer);
                return buffer[0];
            }

            public void WriteByte(byte value)
            {
                Span<byte> buffer = stackalloc byte[] { value };
                Write(buffer);
            }
        }


        public class GpioControllerMock : IGpioController
        {
            private Dictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();

            public int PinCount => 10;

            public void Reset() => _pinValues = new Dictionary<int, PinValue>();

            public void ClosePin(int pinNumber)
            {
            }

            public void Dispose()
            {
            }

            public void OpenPin(int pinNumber, PinMode mode)
            {
            }

            public PinValue Read(int pinNumber)
            {
                if (_pinValues.TryGetValue(pinNumber, out PinValue value))
                    return value;

                return PinValue.Low;
            }

            public void Read(Span<PinValuePair> pinValues)
            {
                for (int i = 0; i < pinValues.Length; i++)
                {
                    int pin = pinValues[i].PinNumber;
                    pinValues[i] = new PinValuePair(pin, Read(pin));
                }
            }

            public void SetPinMode(int pinNumber, PinMode mode)
            {
            }

            public void Write(int pinNumber, PinValue value)
            {
                _pinValues[pinNumber] = value;
            }

            public void Write(ReadOnlySpan<PinValuePair> pinValues)
            {
                foreach ((int pin, PinValue value) in pinValues)
                {
                    Write(pin, value);
                }
            }

            public void OpenPin(int pinNumber)
            {
            }

            public bool IsPinOpen(int pinNumber) => true;

            public PinMode GetPinMode(int pinNumber) => PinMode.Input;

            public bool IsPinModeSupported(int pinNumber, PinMode mode) => true;
        }
    }
}
