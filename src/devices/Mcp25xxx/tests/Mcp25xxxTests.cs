// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests
{
    public class Mcp25xxxTests
    {
        [Fact]
        public void Send_Reset_Instruction()
        {
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.Reset();
            Assert.Equal(0b1100_0000, mcp25xxxSpiDevice.LastWriteByte);
        }

        [Theory]
        [InlineData(Address.CanCtrl)]
        [InlineData(Address.TxB0D0)]
        public void Send_Read_Instruction_By_Address(Address address)
        {
            byte[] expectedWriteBuffer = new byte[] { 0b0000_0011, (byte)address, 0b0000_0000 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.Read(address);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(3, mcp25xxxSpiDevice.LastReadBuffer.Length);
        }

        [Theory]
        [InlineData(0b1001_0000, RxBufferAddressPointer.RxB0Sidh, 1)]
        [InlineData(0b1001_0010, RxBufferAddressPointer.RxB0D0, 4)]
        [InlineData(0b1001_0100, RxBufferAddressPointer.RxB1Sidh, 8)]
        [InlineData(0b1001_0110, RxBufferAddressPointer.RxB1D0, 16)]
        public void Send_ReadRxBuffer_Instruction(byte instructionFormat, RxBufferAddressPointer addressPointer, int byteCount)
        {
            byte[] expectedWriteBuffer = new byte[byteCount + 1];
            expectedWriteBuffer[0] = instructionFormat;
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.ReadRxBuffer(addressPointer, byteCount);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(byteCount, mcp25xxxSpiDevice.LastReadBuffer.Length - 1);
        }

        [Theory]
        [InlineData(Address.CanCtrl, new byte[] { 0b1001_0110 })]
        [InlineData(Address.TxB0D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101 })]
        public void Send_Write_Instruction_By_Address(Address address, byte[] buffer)
        {
            byte[] expectedWriteBuffer = new byte[buffer.Length + 2];
            expectedWriteBuffer[0] = 0b0000_0010;
            expectedWriteBuffer[1] = (byte)address;
            buffer.CopyTo(expectedWriteBuffer, 2);
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.Write(address, buffer);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }

        [Theory]
        [InlineData(0b0100_0000, TxBufferAddressPointer.TxB0Sidh, new byte[] { 0b0000_0001 })]
        [InlineData(0b0100_0101, TxBufferAddressPointer.TxB2D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 })]
        public void Send_LoadTxBuffer_Instruction(byte instructionFormat, TxBufferAddressPointer addressPointer, byte[] buffer)
        {
            byte[] expectedWriteBuffer = new byte[buffer.Length + 1];
            expectedWriteBuffer[0] = instructionFormat;
            buffer.CopyTo(expectedWriteBuffer, 1);
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.LoadTxBuffer(addressPointer, buffer);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(buffer.Length, mcp25xxxSpiDevice.LastWriteBuffer.Length - 1);
        }

        [Theory]
        [InlineData(false, false, false, 0b1000_0000)]
        [InlineData(true, false, false, 0b1000_0001)]
        [InlineData(false, true, false, 0b1000_0010)]
        [InlineData(false, false, true, 0b1000_0100)]
        public void Send_RequestToSend_Instruction(bool txb0, bool txb1, bool txb2, byte expectedByte)
        {
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.RequestToSend(txb0, txb1, txb2);
            Assert.Equal(expectedByte, mcp25xxxSpiDevice.LastWriteByte);
        }

        [Fact]
        public void Send_ReadStatus_Instruction()
        {
            byte[] expectedWriteBuffer = new byte[] { 0b1010_0000, 0b0000_0000};
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.ReadStatus();
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(1, mcp25xxxSpiDevice.LastReadBuffer.Length - 1);
        }

        [Fact]
        public void Send_RxStatus_Instruction()
        {
            byte[] expectedWriteBuffer = new byte[] { 0b1011_0000, 0b0000_0000 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.RxStatus();
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(1, mcp25xxxSpiDevice.LastReadBuffer.Length - 1);
        }

        [Theory]
        [InlineData(Address.CanIntE, 0b0101_1010, 0b0110_1001)]
        [InlineData(Address.CanIntF, 0b1010_0101, 0b1001_0110)]
        public void Send_BitModify_Instruction(Address address, byte mask, byte value)
        {
            byte[] expectedWriteBuffer = new byte[] { 0b0000_0101, (byte)address, mask, value };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.BitModify(address, mask, value);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }
    }
}
