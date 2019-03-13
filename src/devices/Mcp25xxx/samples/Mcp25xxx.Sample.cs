// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;

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
                ReadAllRegistersWithDetails(mcp25xxx);
                // ReadRxBuffer(mcp25xxx);
                // Write(mcp25xxx);
                // LoadTxBuffer(mcp25xxx);
                // RequestToSend(mcp25xxx);
                // ReadStatus(mcp25xxx);
                // RxStatus(mcp25xxx);
                // BitModify(mcp25xxx);
                // TransmitMessage(mcp25xxx);
                //LoopbackMode(mcp25xxx);
                //ReadAllRegisters(mcp25xxx);
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
            Array addresses = Enum.GetValues(typeof(Address));

            foreach (Address address in addresses)
            {
                byte addressData = mcp25xxx.Read(address);
                Console.WriteLine($"0x{(byte)address:X2} - {address,-10}: 0x{addressData:X2}");
            }
        }

        private static void ReadAllRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Read All Registers");
            Console.WriteLine();
            ReadAllMessageTransmitRegistersWithDetails(mcp25xxx);
            ReadAllMessageReceiveRegistersWithDetails(mcp25xxx);
            ReadAllAcceptanceFilterRegistersWithDetails(mcp25xxx);
            ReadAllBitTimeConfigurationRegistersWithDetails(mcp25xxx);
            ReadAllErrorDetectionRegistersWithDetails(mcp25xxx);
            ReadAllInterruptRegistersWithDetails(mcp25xxx);
            ReadAllCanControlRegistersWithDetails(mcp25xxx);
        }

        private static byte ConsoleWriteRegisterAddressDetails(Mcp25xxx mcp25xxx, Address address)
        {
            byte value = mcp25xxx.Read(address);
            Console.WriteLine($"  0x{(byte)address:X2} - {address}: 0x{value:X2}");
            return value;
        }

        private static void ConsoleWriteRegisterItemDetails(string name, string details)
        {
            Console.WriteLine($"{name,15}: {details}");
        }

        private static void ReadAllMessageTransmitRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Message Transmit Registers");

            var txRtsCtrl = new TxRtsCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxRtsCtrl));
            ConsoleWriteRegisterItemDetails("B0RTSM", $"{txRtsCtrl.B0Rtsm}");
            ConsoleWriteRegisterItemDetails("B1RTSM", $"{txRtsCtrl.B1Rtsm}");
            ConsoleWriteRegisterItemDetails("B2RTSM", $"{txRtsCtrl.B2Rtsm}");
            ConsoleWriteRegisterItemDetails("B0RTS", $"{txRtsCtrl.B0Rts}");
            ConsoleWriteRegisterItemDetails("B1RTS", $"{txRtsCtrl.B1Rts}");
            ConsoleWriteRegisterItemDetails("B2RTS", $"{txRtsCtrl.B2Rts}");

            var txB0Ctrl = new TxBxCtrl(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Ctrl));
            ConsoleWriteRegisterItemDetails("TXP[1:0]", $"{(byte)txB0Ctrl.Txp} - { txB0Ctrl.Txp}");
            ConsoleWriteRegisterItemDetails("TXREQ", $"{txB0Ctrl.TxReq}");
            ConsoleWriteRegisterItemDetails("TXERR", $"{txB0Ctrl.TxErr}");
            ConsoleWriteRegisterItemDetails("MLOA", $"{txB0Ctrl.Mloa}");
            ConsoleWriteRegisterItemDetails("ABTF", $"{txB0Ctrl.Abtf}");

            var txB0Sidh = new TxBxSidh(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{txB0Sidh.Sid:X2}");

            var txB0Sidl = new TxBxSidl(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{txB0Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{txB0Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{txB0Sidl.Sid:X2}");

            var txB0Eid8 = new TxBxEid8(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{txB0Eid8.Eid:X2}");

            var txB0Eid0 = new TxBxEid0(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{txB0Eid0.Eid:X2}");

            var txB0Dlc = new TxBxDlc(TxBufferNumber.Zero, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Dlc));
            ConsoleWriteRegisterItemDetails("DLC[3:0]", $"0x{txB0Dlc.Dlc:X2}");
            ConsoleWriteRegisterItemDetails("RTR", $"{txB0Dlc.Rtr}");

            new TxBxDn(TxBufferNumber.Zero, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D0));
            new TxBxDn(TxBufferNumber.Zero, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D1));
            new TxBxDn(TxBufferNumber.Zero, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D2));
            new TxBxDn(TxBufferNumber.Zero, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D3));
            new TxBxDn(TxBufferNumber.Zero, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D4));
            new TxBxDn(TxBufferNumber.Zero, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D5));
            new TxBxDn(TxBufferNumber.Zero, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D6));
            new TxBxDn(TxBufferNumber.Zero, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D7));

            var txB1Ctrl = new TxBxCtrl(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Ctrl));
            ConsoleWriteRegisterItemDetails("TXP[1:0]", $"{(byte)txB1Ctrl.Txp} - { txB1Ctrl.Txp}");
            ConsoleWriteRegisterItemDetails("TXREQ", $"{txB1Ctrl.TxReq}");
            ConsoleWriteRegisterItemDetails("TXERR", $"{txB1Ctrl.TxErr}");
            ConsoleWriteRegisterItemDetails("MLOA", $"{txB1Ctrl.Mloa}");
            ConsoleWriteRegisterItemDetails("ABTF", $"{txB1Ctrl.Abtf}");

            var txB1Sidh = new TxBxSidh(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{txB1Sidh.Sid:X2}");

            var txB1Sidl = new TxBxSidl(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0X{txB1Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{txB1Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{txB1Sidl.Sid:X2}");

            var txB1Eid8 = new TxBxEid8(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{txB1Eid8.Eid:X2}");

            var txB1Eid0 = new TxBxEid0(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{txB1Eid0.Eid:X2}");

            var txB1Dlc = new TxBxDlc(TxBufferNumber.One, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Dlc));
            ConsoleWriteRegisterItemDetails("DLC[3:0]", $"0x{txB1Dlc.Dlc:X2}");
            ConsoleWriteRegisterItemDetails("RTR", $"{txB1Dlc.Rtr}");

            new TxBxDn(TxBufferNumber.One, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D0));
            new TxBxDn(TxBufferNumber.One, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D1));
            new TxBxDn(TxBufferNumber.One, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D2));
            new TxBxDn(TxBufferNumber.One, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D3));
            new TxBxDn(TxBufferNumber.One, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D4));
            new TxBxDn(TxBufferNumber.One, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D5));
            new TxBxDn(TxBufferNumber.One, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D6));
            new TxBxDn(TxBufferNumber.One, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D7));

            var txB2Ctrl = new TxBxCtrl(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Ctrl));
            ConsoleWriteRegisterItemDetails("TXP[1:0]", $"{(byte)txB2Ctrl.Txp} - { txB2Ctrl.Txp}");
            ConsoleWriteRegisterItemDetails("TXREQ", $"{txB2Ctrl.TxReq}");
            ConsoleWriteRegisterItemDetails("TXERR", $"{txB2Ctrl.TxErr}");
            ConsoleWriteRegisterItemDetails("MLOA", $"{txB2Ctrl.Mloa}");
            ConsoleWriteRegisterItemDetails("ABTF", $"{txB2Ctrl.Abtf}");

            var txB2Sidh = new TxBxSidh(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{txB2Sidh.Sid:X2}");

            var txB2Sidl = new TxBxSidl(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{txB2Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{txB2Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{txB2Sidl.Sid:X2}");

            var txB2Eid8 = new TxBxEid8(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{txB2Eid8.Eid:X2}");

            var txB2Eid0 = new TxBxEid0(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{txB2Eid0.Eid:X2}");

            var txB2Dlc = new TxBxDlc(TxBufferNumber.Two, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Dlc));
            ConsoleWriteRegisterItemDetails("DLC[3:0]", $"0x{txB2Dlc.Dlc:X2}");
            ConsoleWriteRegisterItemDetails("RTR", $"{txB2Dlc.Rtr}");

            new TxBxDn(TxBufferNumber.Two, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D0));
            new TxBxDn(TxBufferNumber.Two, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D1));
            new TxBxDn(TxBufferNumber.Two, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D2));
            new TxBxDn(TxBufferNumber.Two, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D3));
            new TxBxDn(TxBufferNumber.Two, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D4));
            new TxBxDn(TxBufferNumber.Two, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D5));
            new TxBxDn(TxBufferNumber.Two, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D6));
            new TxBxDn(TxBufferNumber.Two, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D7));
        }

        private static void ReadAllMessageReceiveRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Message Receive Registers");

            var bfpCtrl = new BfpCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.BfpCtrl));
            ConsoleWriteRegisterItemDetails("B0BFM", $"{bfpCtrl.B0Bfm}");
            ConsoleWriteRegisterItemDetails("B1BFM", $"{bfpCtrl.B1Bfm}");
            ConsoleWriteRegisterItemDetails("B0BFE", $"{bfpCtrl.B0Bfm}");
            ConsoleWriteRegisterItemDetails("B1BFE", $"{bfpCtrl.B1Bfm}");
            ConsoleWriteRegisterItemDetails("B0BFS", $"{bfpCtrl.B0Bfe}");
            ConsoleWriteRegisterItemDetails("B1BFS", $"{bfpCtrl.B1Bfe}");

            var rxB0Ctrl = new RxB0Ctrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Ctrl));
            ConsoleWriteRegisterItemDetails("FILHIT0", $"{rxB0Ctrl.FilHit0}");
            ConsoleWriteRegisterItemDetails("BUKT1", $"{rxB0Ctrl.Bukt1}");
            ConsoleWriteRegisterItemDetails("BUKT", $"{rxB0Ctrl.Bukt}");
            ConsoleWriteRegisterItemDetails("RXRTR", $"{rxB0Ctrl.RxRtr}");
            ConsoleWriteRegisterItemDetails("RXM", $"{(byte)rxB0Ctrl.Rxm} - { rxB0Ctrl.Rxm}");

            var rxB1Ctrl = new RxB1Ctrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Ctrl));
            ConsoleWriteRegisterItemDetails("FILHIT[2:0]", $"{rxB1Ctrl.FilHit}");
            ConsoleWriteRegisterItemDetails("RXRTR", $"{rxB1Ctrl.RxRtr}");
            ConsoleWriteRegisterItemDetails("RXM", $"{(byte)rxB1Ctrl.Rxm} - { rxB1Ctrl.Rxm}");

            var rxB0Sidh = new RxBxSidh(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxB0Sidh.Sid:X2}");

            var rxB0Sidl = new RxBxSidl(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxB0Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("IDE", $"{rxB0Sidl.Ide}");
            ConsoleWriteRegisterItemDetails("SRR", $"{rxB0Sidl.Srr}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxB0Sidl.Sid:X2}");

            var rxB0Eid8 = new RxBxEid8(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxB0Eid8.Eid:X2}");

            var rxB0Eid0 = new RxBxEid0(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxB0Eid0.Eid:X2}");

            var rxB0Dlc = new RxBxDlc(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Dlc));
            ConsoleWriteRegisterItemDetails("DLC[3:0]", $"0x{rxB0Dlc.Dlc:X2}");
            ConsoleWriteRegisterItemDetails("RTR", $"{rxB0Dlc.Rtr}");

            new RxBxDn(0, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D0));
            new RxBxDn(0, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D1));
            new RxBxDn(0, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D2));
            new RxBxDn(0, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D3));
            new RxBxDn(0, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D4));
            new RxBxDn(0, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D5));
            new RxBxDn(0, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D6));
            new RxBxDn(0, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D7));

            var rxB1Sidh = new RxBxSidh(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxB1Sidh.Sid:X2}");

            var rxB1Sidl = new RxBxSidl(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxB1Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("IDE", $"{rxB1Sidl.Ide}");
            ConsoleWriteRegisterItemDetails("SRR", $"{rxB1Sidl.Srr}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxB1Sidl.Sid:X2}");

            var rxB1Eid8 = new RxBxEid8(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxB1Eid8.Eid:X2}");

            var rxB1Eid0 = new RxBxEid0(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxB1Eid0.Eid:X2}");

            var rxB1Dlc = new RxBxDlc(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Dlc));
            ConsoleWriteRegisterItemDetails("DLC[3:0]", $"0x{rxB1Dlc.Dlc:X2}");
            ConsoleWriteRegisterItemDetails("RTR", $"{rxB1Dlc.Rtr}");

            new RxBxDn(1, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D0));
            new RxBxDn(1, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D1));
            new RxBxDn(1, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D2));
            new RxBxDn(1, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D3));
            new RxBxDn(1, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D4));
            new RxBxDn(1, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D5));
            new RxBxDn(1, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D6));
            new RxBxDn(1, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D7));
        }

        private static void ReadAllAcceptanceFilterRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Acceptance Filter Registers");

            var rxF0Sidh = new RxFxSidh(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF0Sidh.Sid:X2}");

            var rxF0Sidl = new RxFxSidl(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF0Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF0Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF0Sidl.Sid:X2}");

            var rxF0Eid8 = new RxFxEid8(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF0Eid8.Eid:X2}");

            var rxF0Eid0 = new RxFxEid0(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF0Eid0.Eid:X2}");

            var rxF1Sidh = new RxFxSidh(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF1Sidh.Sid:X2}");

            var rxF1Sidl = new RxFxSidl(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF1Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF1Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF1Sidl.Sid:X2}");

            var rxF1Eid8 = new RxFxEid8(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF1Eid8.Eid:X2}");

            var rxF1Eid0 = new RxFxEid0(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF1Eid0.Eid:X2}");

            var rxF2Sidh = new RxFxSidh(2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF2Sidh.Sid:X2}");

            var rxF2Sidl = new RxFxSidl(2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF2Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF2Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF2Sidl.Sid:X2}");

            var rxF2Eid8 = new RxFxEid8(2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF2Eid8.Eid:X2}");

            var rxF2Eid0 = new RxFxEid0(2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF2Eid0.Eid:X2}");

            var rxF3Sidh = new RxFxSidh(3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF3Sidh.Sid:X2}");

            var rxF3Sidl = new RxFxSidl(3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF3Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF3Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF3Sidl.Sid:X2}");

            var rxF3Eid8 = new RxFxEid8(3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF3Eid8.Eid:X2}");

            var rxF3Eid0 = new RxFxEid0(3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF3Eid0.Eid:X2}");

            var rxF4Sidh = new RxFxSidh(4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF4Sidh.Sid:X2}");

            var rxF4Sidl = new RxFxSidl(4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF4Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF4Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF4Sidl.Sid:X2}");

            var rxF4Eid8 = new RxFxEid8(4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF4Eid8.Eid:X2}");

            var rxF4Eid0 = new RxFxEid0(4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF4Eid0.Eid:X2}");

            var rxF5Sidh = new RxFxSidh(5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxF5Sidh.Sid:X2}");

            var rxF5Sidl = new RxFxSidl(5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxF5Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("EXIDE", $"{rxF5Sidl.Exide}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxF5Sidl.Sid:X2}");

            var rxF5Eid8 = new RxFxEid8(5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxF5Eid8.Eid:X2}");

            var rxF5Eid0 = new RxFxEid0(5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxF5Eid0.Eid:X2}");










            var rxM0Sidh = new RxMxSidh(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxM0Sidh.Sid:X2}");

            var rxM0Sidl = new RxMxSidl(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxM0Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxM0Sidl.Sid:X2}");

            var rxM0Eid8 = new RxMxEid8(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxM0Eid8.Eid:X2}");

            var rxM0Eid0 = new RxMxEid0(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxM0Eid0.Eid:X2}");

            var rxM1Sidh = new RxMxSidh(0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Sidh));
            ConsoleWriteRegisterItemDetails("SID[10:3]", $"0x{rxM1Sidh.Sid:X2}");

            var rxM1Sidl = new RxMxSidl(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Sidl));
            ConsoleWriteRegisterItemDetails("EID[17:16]", $"0x{rxM1Sidl.Eid:X2}");
            ConsoleWriteRegisterItemDetails("SID[2:0]", $"0x{rxM1Sidl.Sid:X2}");

            var rxM1Eid8 = new RxMxEid8(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Eid8));
            ConsoleWriteRegisterItemDetails("EID[15:8]", $"0x{rxM1Eid8.Eid:X2}");

            var rxM1Eid0 = new RxMxEid0(1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Eid0));
            ConsoleWriteRegisterItemDetails("EID[7:0]", $"0x{rxM1Eid0.Eid:X2}");

            //
            // Acceptance Filter Registers.
            //
            //RxF0Sidh = 0x00,
            //RxF0Sidl = 0x01,
            //RxF0Eid8 = 0x02,
            //RxF0Eid0 = 0x03,
            //RxF1Sidh = 0x04,
            //RxF1Sidl = 0x05,
            //RxF1Eid8 = 0x06,
            //RxF1Eid0 = 0x07,
            //RxF2Sidh = 0x08,
            //RxF2Sidl = 0x09,
            //RxF2Eid8 = 0x0A,
            //RxF2Eid0 = 0x0B,
            //RxF3Sidh = 0x10,
            //RxF3Sidl = 0x11,
            //RxF3Eid8 = 0x12,
            //RxF3Eid0 = 0x13,
            //RxF4Sidh = 0x14,
            //RxF4Sidl = 0x15,
            //RxF4Eid8 = 0x16,
            //RxF4Eid0 = 0x17,
            //RxF5Sidh = 0x18,
            //RxF5Sidl = 0x19,
            //RxF5Eid8 = 0x1A,
            //RxF5Eid0 = 0x1B,
            //RxM0Sidh = 0x20,
            //RxM0Sidl = 0x21,
            //RxM0Eid8 = 0x22,
            //RxM0Eid0 = 0x23,
            //RxM1Sidh = 0x24,
            //RxM1Sidl = 0x25,
            //RxM1Eid8 = 0x26,
            //RxM1Eid0 = 0x27,
        }

        private static void ReadAllBitTimeConfigurationRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Bit Time Configuration Registers");

            var cnf1 = new Cnf1(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf1));
            ConsoleWriteRegisterItemDetails("BRP[5:0]", $"0x{cnf1.Brp:X2}");
            ConsoleWriteRegisterItemDetails("SJW[1:0]", $"{(byte)cnf1.Sjw} - {cnf1.Sjw}");

            var cnf2 = new Cnf2(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf2));
            ConsoleWriteRegisterItemDetails("PRSEG[2:0]", $"0x{cnf2.PrSeg:X2}");
            ConsoleWriteRegisterItemDetails("PHSEG1[2:0]", $"0x{cnf2.PhSeg1:X2}");
            ConsoleWriteRegisterItemDetails("SAM", $"{cnf2.Sam}");
            ConsoleWriteRegisterItemDetails("BTLMODE", $"{cnf2.BtlMode}");

            var cnf3 = new Cnf3(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf3));
            ConsoleWriteRegisterItemDetails("PHSEG2[2:0]", $"0x{cnf3.PhSeg2:X2}");
            ConsoleWriteRegisterItemDetails("WAKFIL", $"{cnf3.WakFil}");
            ConsoleWriteRegisterItemDetails("SOF", $"{cnf3.Sof}");
        }

        private static void ReadAllErrorDetectionRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Error Detection Registers");

            var tec = new Tec(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Tec));
            ConsoleWriteRegisterItemDetails("TEC[7:0]", $"0x{tec.Data:X2}");

            var rec = new Rec(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Rec));
            ConsoleWriteRegisterItemDetails("REC[7:0]", $"0x{rec.Data:X2}");

            var eflg = new Eflg(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Eflg));
            ConsoleWriteRegisterItemDetails("EWARN", $"{eflg.Ewarn}");
            ConsoleWriteRegisterItemDetails("RXWAR", $"{eflg.RxWar}");
            ConsoleWriteRegisterItemDetails("TXWAR", $"{eflg.TxWar}");
            ConsoleWriteRegisterItemDetails("RXEP", $"{eflg.RxEp}");
            ConsoleWriteRegisterItemDetails("TXEP", $"{eflg.TxEp}");
            ConsoleWriteRegisterItemDetails("TXBO", $"{eflg.TxBo}");
            ConsoleWriteRegisterItemDetails("RX0OVR", $"{eflg.Rx0Ovr}");
            ConsoleWriteRegisterItemDetails("RX1OVR", $"{eflg.Rx1Ovr}");
        }

        private static void ReadAllInterruptRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Interrupt Registers");

            var canIntE = new CanIntE(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanIntE));
            ConsoleWriteRegisterItemDetails("RX0IE", $"{canIntE.Rx0Ie}");
            ConsoleWriteRegisterItemDetails("RX1IE", $"{canIntE.Rx1Ie}");
            ConsoleWriteRegisterItemDetails("TX0IE", $"{canIntE.Tx0Ie}");
            ConsoleWriteRegisterItemDetails("TX1IE", $"{canIntE.Tx1Ie}");
            ConsoleWriteRegisterItemDetails("TX2IE", $"{canIntE.Tx2Ie}");
            ConsoleWriteRegisterItemDetails("ERRIE", $"{canIntE.ErrIe}");
            ConsoleWriteRegisterItemDetails("WAKIE", $"{canIntE.WakIe}");
            ConsoleWriteRegisterItemDetails("MERRE", $"{canIntE.Merre}");

            var canIntF = new CanIntF(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanIntF));
            ConsoleWriteRegisterItemDetails("RX0IF", $"{canIntF.Rx0If}");
            ConsoleWriteRegisterItemDetails("RX1IF", $"{canIntF.Rx1If}");
            ConsoleWriteRegisterItemDetails("TX0IF", $"{canIntF.Tx0If}");
            ConsoleWriteRegisterItemDetails("TX1IF", $"{canIntF.Tx1If}");
            ConsoleWriteRegisterItemDetails("TX2IF", $"{canIntF.Tx2If}");
            ConsoleWriteRegisterItemDetails("ERRIF", $"{canIntF.ErrIf}");
            ConsoleWriteRegisterItemDetails("WAKIF", $"{canIntF.WakIf}");
            ConsoleWriteRegisterItemDetails("MERRF", $"{canIntF.Merrf}");
        }

        private static void ReadAllCanControlRegistersWithDetails(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("CAN Control Registers");

            var canCtrl = new CanCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanCtrl));
            ConsoleWriteRegisterItemDetails("CLKPRE[1:0]", $"{(byte)canCtrl.ClkPre} - {canCtrl.ClkPre}");
            ConsoleWriteRegisterItemDetails("CLKEN", $"{canCtrl.ClkEn}");
            ConsoleWriteRegisterItemDetails("OSM", $"{canCtrl.Osm}");
            ConsoleWriteRegisterItemDetails("ABAT", $"{canCtrl.Abat}");
            ConsoleWriteRegisterItemDetails("REQOP[2:0]", $"{(byte)canCtrl.ReqOp} - {canCtrl.ReqOp}");

            var canStat = new CanStat(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanStat));
            ConsoleWriteRegisterItemDetails("ICOD[2:0]", $"{(byte)canStat.Icod} - {canStat.Icod}");
            ConsoleWriteRegisterItemDetails("OPMOD[2:0]", $"{(byte)canStat.OpMod} - {canStat.OpMod}");
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
            mcp25xxx.Write(Address.CanCtrl, new byte[] { 0b1001_1111 });
            mcp25xxx.Write(Address.TxB0D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 });
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
            Console.WriteLine($"Value: 0x{readStatusResponse:X2}");
            Console.WriteLine($"Rx0If: {readStatusResponse.HasFlag(ReadStatusResponse.Rx0If)}");
            Console.WriteLine($"Rx1If: {readStatusResponse.HasFlag(ReadStatusResponse.Rx1If)}");
            Console.WriteLine($"Tx0Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req)}");
            Console.WriteLine($"Tx0If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0If)}");
            Console.WriteLine($"Tx0Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req)}");
            Console.WriteLine($"Tx1If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx1If)}");
            Console.WriteLine($"Tx1Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx1Req)}");
            Console.WriteLine($"Tx2Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx2Req)}");
            Console.WriteLine($"Tx2If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx2If)}");
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
            mcp25xxx.BitModify(Address.CanIntE, 0b1010_0110, 0b1111_1111);
        }

        private static void TransmitMessage(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Transmit Message");

            mcp25xxx.WriteByte(
                new CanCtrl(CanCtrl.ClkOutPinPrescaler.ClockDivideBy8,
                false,
                false,
                false,
                Tests.Register.CanControl.OperationMode.NormalOperation));

            byte[] data = new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111, 0b1000_1001 };

            mcp25xxx.Write(
                Address.TxB0Sidh,
                new byte[]
                {
                    new TxBxSidh(TxBufferNumber.Zero, 0b0000_1001).ToByte(),
                    new TxBxSidl(TxBufferNumber.Zero, 0b001, false, 0b00).ToByte(),
                    new TxBxEid8(TxBufferNumber.Zero, 0b0000_0000).ToByte(),
                    new TxBxEid0(TxBufferNumber.Zero, 0b0000_0000).ToByte(),
                    new TxBxDlc(TxBufferNumber.Zero, data.Length, false).ToByte()
                });

            mcp25xxx.Write(Address.TxB0D0, data);

            // Send with TxB0 buffer.
            mcp25xxx.RequestToSend(true, false, false);
        }

        private static void LoopbackMode(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Loopback Mode");
            mcp25xxx.WriteByte(
                new CanCtrl(
                    CanCtrl.ClkOutPinPrescaler.ClockDivideBy8,
                    true,
                    false,
                    false,
                    Tests.Register.CanControl.OperationMode.Loopback));
        }
    }
}
