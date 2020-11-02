// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class OpCodeTests
    {
        [Theory]
        // Writing
        [InlineData(0x20, false, 0b0100_0000)]
        [InlineData(0x21, false, 0b0100_0010)]
        [InlineData(0x22, false, 0b0100_0100)]
        [InlineData(0x23, false, 0b0100_0110)]
        [InlineData(0x24, false, 0b0100_1000)]
        [InlineData(0x25, false, 0b0100_1010)]
        [InlineData(0x26, false, 0b0100_1100)]
        [InlineData(0x27, false, 0b0100_1110)]
        // Reading
        [InlineData(0x20, true, 0b0100_0001)]
        [InlineData(0x21, true, 0b0100_0011)]
        [InlineData(0x22, true, 0b0100_0101)]
        [InlineData(0x23, true, 0b0100_0111)]
        [InlineData(0x24, true, 0b0100_1001)]
        [InlineData(0x25, true, 0b0100_1011)]
        [InlineData(0x26, true, 0b0100_1101)]
        [InlineData(0x27, true, 0b0100_1111)]
        public void Get_OpCode(int deviceAddress, bool isReadCommand, byte expectedOpCode)
        {
            SpiMock spiMock = new SpiMock();

            // The Mcp23s17 is the only SPI device that supports all 8 addresses
            Mcp23s17 mcp23S08 = new Mcp23s17(spiMock, deviceAddress);
            if (isReadCommand)
            {
                mcp23S08.ReadByte(Register.GPIO);
            }
            else
            {
                mcp23S08.WriteByte(Register.GPIO, 0xA1);
            }

            Assert.Equal(expectedOpCode, spiMock.LastInitialWriteByte);
        }

        private class SpiMock : SpiDevice
        {
            public byte LastInitialWriteByte { get; private set; }

            public override SpiConnectionSettings ConnectionSettings => null;

            public override void Read(Span<byte> buffer)
            {
            }

            public override byte ReadByte() => 0x42;

            public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
            {
                LastInitialWriteByte = writeBuffer[0];
            }

            public override void Write(ReadOnlySpan<byte> data)
            {
                LastInitialWriteByte = data[0];
            }

            public override void WriteByte(byte data)
            {
            }
        }
    }
}
