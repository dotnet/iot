using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;
using Iot.Device.Bmxx80.Tests;

namespace Iot.Device.Imu.Tests
{
    public class SimulatedI2cDevice : I2cDevice
    {
        private byte[] _registers;
        private byte _currentRegister;

        public SimulatedI2cDevice(I2cConnectionSettings connectionSettings)
        {
            _registers = new byte[256];
            _currentRegister = 0;
            ConnectionSettings = connectionSettings;
        }

        public SimulatedI2cDevice()
            : this(new I2cConnectionSettings(1, 1))
        {
        }

        public override I2cConnectionSettings ConnectionSettings { get; }

        public override byte ReadByte()
        {
            return _registers[_currentRegister];
        }

        public override void Read(Span<byte> buffer)
        {
            int idx = _currentRegister;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = _registers[idx];
                idx++;
            }
        }

        public override void WriteByte(byte value)
        {
            _currentRegister = value;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _currentRegister = buffer[0];
            int idx = _currentRegister;

            for (int i = 1; i < buffer.Length; i++)
            {
                _registers[idx] = buffer[i];
                idx++;
            }
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(writeBuffer);
            Read(readBuffer);
        }

        public void SetRegister(int register, byte value)
        {
            _registers[register] = value;
        }

        public void SetRegister(int register, short value)
        {
            _registers[register] = (byte)(value & 0xFF);
            _registers[register + 1] = (byte)(value >> 8);
        }

        public void SetRegister(int register, ushort value)
        {
            _registers[register] = (byte)(value & 0xFF);
            _registers[register + 1] = (byte)(value >> 8);
        }
    }
}
