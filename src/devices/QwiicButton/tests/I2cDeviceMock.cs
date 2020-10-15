using System;
using System.Device.I2c;

namespace QwiicButton.Tests
{
    internal class I2cDeviceMock : I2cDevice
    {
        public override byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteBuffer { get; set; }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            WriteBuffer = buffer.ToArray();
        }

        public byte[] ReadBuffer { get; set; }
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            WriteBuffer = writeBuffer.ToArray();

            for (int index = 0; index < ReadBuffer.Length; index++)
            {
                readBuffer[index] = ReadBuffer[index];
            }
        }

        public override I2cConnectionSettings ConnectionSettings { get; }
    }
}
