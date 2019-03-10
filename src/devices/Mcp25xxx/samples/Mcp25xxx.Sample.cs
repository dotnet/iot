// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;
using Iot.Device.Mcp25xxx;

namespace Iot.Device.Mcp25xxx.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mcp25xxx Sample!");

            using (Mcp25xxx mcp25xxx = GetMcp25xxxDevice())
            {
                Reset(mcp25xxx);
                
                // ReadAllRegisters(mcp25xxx);
                // ReadRxBuffer(mcp25xxx);
                // Write(mcp25xxx);
                // LoadTxBuffer(mcp25xxx);
                // RequestToSend(mcp25xxx);
                // ReadStatus(mcp25xxx);
                // RxStatus(mcp25xxx);
                // BitModify(mcp25xxx);
                // TransmitMessage(mcp25xxx);
                //LoopbackMode(mcp25xxx);
                ReadAllRegisters(mcp25xxx);
            }
        }

        private static Mcp25xxx GetMcp25xxxDevice()
        {
            var spiConnectionSettings = new SpiConnectionSettings(0, 0);
            var spiDevice = new UnixSpiDevice(spiConnectionSettings);
            return new Mcp25625(spiDevice);
        }

        private static void Reset(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Reset Instruction");
            mcp25xxx.Reset();
        }

        private static void ReadAllRegisters(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Read Instruction for All Registers");
            Array addresses = Enum.GetValues(typeof(Register.Address));

            foreach (Register.Address address in addresses)
            {
                byte addressData = mcp25xxx.Read(address);
                Console.WriteLine($"0x{(byte)address:X2} - {address,-10}: 0x{addressData:X2}");
            }
        }

        private static void ReadRxBuffer(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Read Rx Buffer Instruction");
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB0Sidh, 1);
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB0Sidh, 5);
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB0D0, 8);
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB1Sidh, 1);
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB1Sidh, 5);
            ReadRxBuffer(mcp25xxx, RxBufferAddressPointer.RxB1D0, 8);
        }

        private static void ReadRxBuffer(Mcp25xxx mcp25xxx, RxBufferAddressPointer addressPointer, int byteCount)
        {
            byte[] data = mcp25xxx.ReadRxBuffer(addressPointer, byteCount);

            Console.Write($"{addressPointer,10}: ");

            foreach (byte value in data)
            {
                Console.Write($"0x{value:X2} ");
            }

            Console.WriteLine();
        }

        private static void Write(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Write Instruction");
            mcp25xxx.Write(Register.Address.CanCtrl, new byte[] { 0b1001_1111 });
            mcp25xxx.Write(Register.Address.TxB0D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 });
        }

        private static void LoadTxBuffer(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Load Tx Buffer Instruction");
            mcp25xxx.LoadTxBuffer(TxBufferAddressPointer.TxB0D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 });
            mcp25xxx.LoadTxBuffer(TxBufferAddressPointer.TxB0Sidh, new byte[] { 0b1001_0110 });
        }

        private static void RequestToSend(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Request-to-Send Instruction");
            mcp25xxx.RequestToSend(false, false, false);
            mcp25xxx.RequestToSend(true, false, false);
            mcp25xxx.RequestToSend(false, true, false);
            mcp25xxx.RequestToSend(false, false, true);
        }

        private static void ReadStatus(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Read Status Instruction");
            ReadStatusResponse readStatusResponse = mcp25xxx.ReadStatus();
            Console.WriteLine($"Value: 0x{readStatusResponse.ToByte():X2}");
            Console.WriteLine($"Rx0If: {readStatusResponse.Rx0If}");
            Console.WriteLine($"Rx1If: {readStatusResponse.Rx1If}");
            Console.WriteLine($"Tx0Req: {readStatusResponse.Tx0Req}");
            Console.WriteLine($"Tx0If: {readStatusResponse.Tx0If}");
            Console.WriteLine($"Tx0Req: {readStatusResponse.Tx0Req}");
            Console.WriteLine($"Tx1If: {readStatusResponse.Tx1If}");
            Console.WriteLine($"Tx1Req: {readStatusResponse.Tx1Req}");
            Console.WriteLine($"Tx2Req: {readStatusResponse.Tx2Req}");
            Console.WriteLine($"Tx2If: {readStatusResponse.Tx2If}");
        }

        private static void RxStatus(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Rx Status Instruction");
            RxStatusResponse rxStatusResponse = mcp25xxx.RxStatus();
            Console.WriteLine($"Value: 0x{rxStatusResponse.ToByte():X2}");
            Console.WriteLine($"Filter Match: {rxStatusResponse.FilterMatch}");
            Console.WriteLine($"Message Type Received: {rxStatusResponse.MessageTypeReceived}");
            Console.WriteLine($"Received Message: {rxStatusResponse.ReceivedMessage}");
        }

        private static void BitModify(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Bit Modify Instruction");
            mcp25xxx.BitModify(Register.Address.CanIntE, 0b1010_0110, 0b1111_1111);
        }

        private static void TransmitMessage(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Transmit Message");

            mcp25xxx.WriteByte(
                new Register.CanControl.CanCtrl(Register.CanControl.CanCtrl.ClkOutPinPrescaler.ClockDivideBy8,
                false,
                false,
                false,
                Tests.Register.CanControl.OperationMode.NormalOperation));

            byte[] data = new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111, 0b1000_1001 };

            mcp25xxx.Write(
                Register.Address.TxB0Sidh,
                new byte[]
                {
                    new Register.MessageTransmit.TxBxSidh(Register.MessageTransmit.TxBufferNumber.Zero, 0b0000_1001).ToByte(),
                    new Register.MessageTransmit.TxBxSidl(Register.MessageTransmit.TxBufferNumber.Zero, 0b001, false, 0b00).ToByte(),
                    new Register.MessageTransmit.TxBxEid8(Register.MessageTransmit.TxBufferNumber.Zero, 0b0000_0000).ToByte(),
                    new Register.MessageTransmit.TxBxEid0(Register.MessageTransmit.TxBufferNumber.Zero, 0b0000_0000).ToByte(),
                    new Register.MessageTransmit.TxBxDlc(Register.MessageTransmit.TxBufferNumber.Zero, data.Length, false).ToByte()
                });

            mcp25xxx.Write(Register.Address.TxB0D0, data);

            // Send with TxB0 buffer.
            mcp25xxx.RequestToSend(true, false, false);
        }

        private static void LoopbackMode(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Loopback Mode");
            mcp25xxx.WriteByte(
                new Register.CanControl.CanCtrl(
                    Register.CanControl.CanCtrl.ClkOutPinPrescaler.ClockDivideBy8,
                    true,
                    false,
                    false,
                    Tests.Register.CanControl.OperationMode.Loopback));
        }
    }
}
