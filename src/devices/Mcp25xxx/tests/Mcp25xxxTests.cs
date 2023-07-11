// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Tests.Register.CanControl;
using Moq;
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
            byte[] reply = new byte[]
            {
                0, 0, 0xff
            };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            mcp25xxxSpiDevice.NextReadBuffer = reply;
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            byte b = mcp25xxx.Read(address);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice?.LastWriteBuffer);
            Assert.Equal(0xff, b);
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
            mcp25xxxSpiDevice.NextReadBuffer = new byte[byteCount];
            mcp25xxx.ReadRxBuffer(addressPointer, byteCount);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
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
            Assert.Equal(buffer.Length, mcp25xxxSpiDevice.LastWriteBuffer?.Length - 1);
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
            byte[] expectedWriteBuffer = new byte[] { 0b1010_0000, 0b0000_0000 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxxSpiDevice.NextReadBuffer = new byte[]
            {
                0, 3
            };

            var response = mcp25xxx.ReadStatus();
            Assert.Equal(ReadStatusResponse.Rx0If | ReadStatusResponse.Rx1If, response);
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }

        [Fact]
        public void Send_RxStatus_Instruction()
        {
            byte[] expectedWriteBuffer = new byte[] { 0b1011_0000, 0b0000_0000 };
            byte[] reply = new byte[]
            {
                0, 0xC2
            };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            mcp25xxxSpiDevice.NextReadBuffer = reply;
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            var status = mcp25xxx.RxStatus();
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
            Assert.Equal(RxStatusResponse.MessageReceivedType.StandardDataFrame, status.MessageTypeReceived);
            Assert.Equal(RxStatusResponse.ReceivedMessageType.MessagesInBothBuffers, status.ReceivedMessage);
            Assert.Equal(RxStatusResponse.FilterMatchType.RxF2, status.FilterMatch);
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

        [Fact]
        public void Send_EnableRollover_Instruction()
        {
            byte[] expectedWriteBuffer = { 0b0000_0010, (byte)Address.RxB0Ctrl, 0b0110_0110 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.EnableRollover();
            Assert.Equal(expectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }

        [Fact]
        public void Send_SetBitrate_Instruction()
        {
            byte[] lastExpectedWriteBuffer = { 0b0000_0010, (byte)Address.Cnf3, 0x86 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.SetBitrate(FrequencyAndSpeed._16MHz500KBps);
            Assert.Equal(lastExpectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }

        [Fact]
        public void Send_SetMode_Instruction()
        {
            byte[] lastExpectedWriteBuffer = { 0b0000_0010, (byte)Address.CanCtrl, 0b0000_0111 };
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxx.SetMode(OperationMode.NormalOperation);
            Assert.Equal(lastExpectedWriteBuffer, mcp25xxxSpiDevice.LastWriteBuffer);
        }

        [Fact]
        public void ReceiveMessagesSuccess()
        {
            var mcp25xxxSpiDevice = new Mcp25xxxSpiDevice();
            mcp25xxxSpiDevice.TransferCompleted += () =>
            {
                // The second reply;
                mcp25xxxSpiDevice.NextReadBuffer = new byte[]
                {
                    0 /* dummy */, 0xb, 0xa, 0, 0, 0x4 /* Msg length*/, 1, 2, 3, 4
                };
            };

            byte[] reply = new byte[]
            {
                0, 0x42
            };
            Mcp25xxx mcp25xxx = new Mcp25625(mcp25xxxSpiDevice);
            mcp25xxxSpiDevice.NextReadBuffer = reply;
            var msg = mcp25xxx.ReadMessages();
            Assert.Single(msg);

            Assert.Equal(0xa0b, BitConverter.ToInt32(msg[0].GetId(), 0));
            Assert.Equal(4, msg[0].GetData().Length);
            Assert.Equal(1, msg[0].GetData()[0]);
        }
    }
}
