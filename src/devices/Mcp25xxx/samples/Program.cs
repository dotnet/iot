// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using Iot.Device.Mcp25xxx;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Iot.Device.Mcp25xxx.Tests.Register.CanControl;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;

Console.WriteLine("Hello Mcp25xxx Sample!");

using Mcp25xxx mcp25xxx = GetMcp25xxxDevice();
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
// LoopbackMode(mcp25xxx);
// ReadAllRegisters(mcp25xxx);
// Methods
Mcp25xxx GetMcp25xxxDevice()
{
    SpiConnectionSettings spiConnectionSettings = new (0, 0);
    SpiDevice spiDevice = SpiDevice.Create(spiConnectionSettings);
    return new Mcp25625(spiDevice);
}

void Reset(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Reset Instruction");
    mcp25xxx.Reset();
}

void ReadAllRegisters(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Read Instruction for All Registers");
    Array addresses = Enum.GetValues(typeof(Address));

    foreach (Address address in addresses)
    {
        byte addressData = mcp25xxx.Read(address);
        Console.WriteLine($"0x{(byte)address:X2} - {address,-10}: 0x{addressData:X2}");
    }
}

void ReadAllRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Read All Registers");
    Console.WriteLine();

    ReadAllMessageTransmitRegistersWithDetails(mcp25xxx);
    // ReadAllMessageReceiveRegistersWithDetails(mcp25xxx);
    // ReadAllAcceptanceFilterRegistersWithDetails(mcp25xxx);
    // ReadAllBitTimeConfigurationRegistersWithDetails(mcp25xxx);
    // ReadAllErrorDetectionRegistersWithDetails(mcp25xxx);
    // ReadAllInterruptRegistersWithDetails(mcp25xxx);
    // ReadAllCanControlRegistersWithDetails(mcp25xxx);
}

byte ConsoleWriteRegisterAddressDetails(Mcp25xxx mcp25xxx, Address address)
{
    byte value = mcp25xxx.Read(address);
    Console.WriteLine($"  0x{(byte)address:X2} - {address}: 0x{value:X2}");
    return value;
}

void ConsoleWriteRegisterItemDetails(IRegister register)
{
    foreach (System.Reflection.PropertyInfo property in register.GetType().GetProperties())
    {
        Console.WriteLine($"{property.Name,15}: {property.GetValue(register, null)}");
    }
}

void ReadAllMessageTransmitRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Message Transmit Registers");

    ConsoleWriteRegisterItemDetails(
        new TxRtsCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxRtsCtrl)));
    ConsoleWriteRegisterItemDetails(new TxBxCtrl(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Ctrl)));
    ConsoleWriteRegisterItemDetails(new TxBxSidh(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Sidh)));
    ConsoleWriteRegisterItemDetails(new TxBxSidl(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Sidl)));
    ConsoleWriteRegisterItemDetails(new TxBxEid8(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Eid8)));
    ConsoleWriteRegisterItemDetails(new TxBxEid0(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Eid0)));
    ConsoleWriteRegisterItemDetails(new TxBxDlc(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0Dlc)));

    new TxBxDn(0, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D0));
    new TxBxDn(0, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D1));
    new TxBxDn(0, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D2));
    new TxBxDn(0, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D3));
    new TxBxDn(0, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D4));
    new TxBxDn(0, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D5));
    new TxBxDn(0, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D6));
    new TxBxDn(0, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB0D7));

    ConsoleWriteRegisterItemDetails(new TxBxCtrl(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Ctrl)));
    ConsoleWriteRegisterItemDetails(new TxBxSidh(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Sidh)));
    ConsoleWriteRegisterItemDetails(new TxBxSidl(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Sidl)));
    ConsoleWriteRegisterItemDetails(new TxBxEid8(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Eid8)));
    ConsoleWriteRegisterItemDetails(new TxBxEid0(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Eid0)));
    ConsoleWriteRegisterItemDetails(new TxBxDlc(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1Dlc)));

    new TxBxDn(1, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D0));
    new TxBxDn(1, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D1));
    new TxBxDn(1, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D2));
    new TxBxDn(1, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D3));
    new TxBxDn(1, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D4));
    new TxBxDn(1, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D5));
    new TxBxDn(1, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D6));
    new TxBxDn(1, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB1D7));

    ConsoleWriteRegisterItemDetails(new TxBxCtrl(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Ctrl)));
    ConsoleWriteRegisterItemDetails(new TxBxSidh(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Sidh)));
    ConsoleWriteRegisterItemDetails(new TxBxSidl(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Sidl)));
    ConsoleWriteRegisterItemDetails(new TxBxEid8(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Eid8)));
    ConsoleWriteRegisterItemDetails(new TxBxEid0(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Eid0)));
    ConsoleWriteRegisterItemDetails(new TxBxDlc(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2Dlc)));

    new TxBxDn(2, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D0));
    new TxBxDn(2, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D1));
    new TxBxDn(2, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D2));
    new TxBxDn(2, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D3));
    new TxBxDn(2, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D4));
    new TxBxDn(2, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D5));
    new TxBxDn(2, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D6));
    new TxBxDn(2, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.TxB2D7));
}

void ReadAllMessageReceiveRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Message Receive Registers");

    ConsoleWriteRegisterItemDetails(new BfpCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.BfpCtrl)));
    ConsoleWriteRegisterItemDetails(
        new RxB0Ctrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Ctrl)));
    ConsoleWriteRegisterItemDetails(
        new RxB1Ctrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Ctrl)));
    ConsoleWriteRegisterItemDetails(new RxBxSidh(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Sidh)));
    ConsoleWriteRegisterItemDetails(new RxBxSidl(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Sidl)));
    ConsoleWriteRegisterItemDetails(new RxBxEid8(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Eid8)));
    ConsoleWriteRegisterItemDetails(new RxBxEid0(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Eid0)));
    ConsoleWriteRegisterItemDetails(new RxBxDlc(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0Dlc)));

    new RxBxDn(0, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D0));
    new RxBxDn(0, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D1));
    new RxBxDn(0, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D2));
    new RxBxDn(0, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D3));
    new RxBxDn(0, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D4));
    new RxBxDn(0, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D5));
    new RxBxDn(0, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D6));
    new RxBxDn(0, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB0D7));

    ConsoleWriteRegisterItemDetails(new RxBxSidh(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Sidh)));
    ConsoleWriteRegisterItemDetails(new RxBxSidl(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Sidl)));
    ConsoleWriteRegisterItemDetails(new RxBxEid8(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Eid8)));
    ConsoleWriteRegisterItemDetails(new RxBxEid0(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Eid0)));
    ConsoleWriteRegisterItemDetails(new RxBxDlc(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1Dlc)));

    new RxBxDn(1, 0, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D0));
    new RxBxDn(1, 1, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D1));
    new RxBxDn(1, 2, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D2));
    new RxBxDn(1, 3, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D3));
    new RxBxDn(1, 4, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D4));
    new RxBxDn(1, 5, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D5));
    new RxBxDn(1, 6, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D6));
    new RxBxDn(1, 7, ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxB1D7));
}

void ReadAllAcceptanceFilterRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Acceptance Filter Registers");

    ConsoleWriteRegisterItemDetails(new RxFxSidh(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF0Eid0)));
    ConsoleWriteRegisterItemDetails(new RxFxSidh(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF1Eid0)));
    ConsoleWriteRegisterItemDetails(new RxFxSidh(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(2,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF2Eid0)));
    ConsoleWriteRegisterItemDetails(new RxFxSidh(3,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(3,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(3,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(3,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF3Eid0)));
    ConsoleWriteRegisterItemDetails(new RxFxSidh(4,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(4,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(4,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(4,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF4Eid0)));
    ConsoleWriteRegisterItemDetails(new RxFxSidh(5,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Sidh)));
    ConsoleWriteRegisterItemDetails(new RxFxSidl(5,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Sidl)));
    ConsoleWriteRegisterItemDetails(new RxFxEid8(5,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Eid8)));
    ConsoleWriteRegisterItemDetails(new RxFxEid0(5,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxF5Eid0)));
    ConsoleWriteRegisterItemDetails(new RxMxSidh(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Sidh)));
    ConsoleWriteRegisterItemDetails(new RxMxSidl(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Sidl)));
    ConsoleWriteRegisterItemDetails(new RxMxEid8(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Eid8)));
    ConsoleWriteRegisterItemDetails(new RxMxEid0(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM0Eid0)));
    ConsoleWriteRegisterItemDetails(new RxMxSidh(0,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Sidh)));
    ConsoleWriteRegisterItemDetails(new RxMxSidl(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Sidl)));
    ConsoleWriteRegisterItemDetails(new RxMxEid8(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Eid8)));
    ConsoleWriteRegisterItemDetails(new RxMxEid0(1,
        ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.RxM1Eid0)));
}

void ReadAllBitTimeConfigurationRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Bit Time Configuration Registers");

    ConsoleWriteRegisterItemDetails(new Cnf1(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf1)));
    ConsoleWriteRegisterItemDetails(new Cnf2(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf2)));
    ConsoleWriteRegisterItemDetails(new Cnf3(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Cnf3)));
}

void ReadAllErrorDetectionRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Error Detection Registers");

    ConsoleWriteRegisterItemDetails(new Tec(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Tec)));
    ConsoleWriteRegisterItemDetails(new Rec(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Rec)));
    ConsoleWriteRegisterItemDetails(new Eflg(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.Eflg)));
}

void ReadAllInterruptRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Interrupt Registers");

    ConsoleWriteRegisterItemDetails(new CanIntE(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanIntE)));
    ConsoleWriteRegisterItemDetails(new CanIntF(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanIntF)));
}

void ReadAllCanControlRegistersWithDetails(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("CAN Control Registers");

    ConsoleWriteRegisterItemDetails(new CanCtrl(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanCtrl)));
    ConsoleWriteRegisterItemDetails(new CanStat(ConsoleWriteRegisterAddressDetails(mcp25xxx, Address.CanStat)));
}

void ReadRxBuffer(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Read Rx Buffer Instruction");
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB0Sidh, 1);
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB0Sidh, 5);
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB0D0, 8);
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB1Sidh, 1);
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB1Sidh, 5);
    ReadRxBufferInteral(mcp25xxx, RxBufferAddressPointer.RxB1D0, 8);
}

void ReadRxBufferInteral(Mcp25xxx mcp25xxx, RxBufferAddressPointer addressPointer, int byteCount)
{
    byte[] data = mcp25xxx.ReadRxBuffer(addressPointer, byteCount);

    Console.Write($"{addressPointer,10}: ");

    foreach (byte value in data)
    {
        Console.Write($"0x{value:X2} ");
    }

    Console.WriteLine();
}

void Write(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Write Instruction");
    mcp25xxx.Write(Address.CanCtrl, new byte[] { 0b1001_1111 });
    mcp25xxx.Write(Address.TxB0D0, new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 });
}

void LoadTxBuffer(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Load Tx Buffer Instruction");
    mcp25xxx.LoadTxBuffer(TxBufferAddressPointer.TxB0D0,
        new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111 });
    mcp25xxx.LoadTxBuffer(TxBufferAddressPointer.TxB0Sidh, new byte[] { 0b1001_0110 });
}

void RequestToSend(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Request-to-Send Instruction");
    mcp25xxx.RequestToSend(false, false, false);
    mcp25xxx.RequestToSend(true, false, false);
    mcp25xxx.RequestToSend(false, true, false);
    mcp25xxx.RequestToSend(false, false, true);
}

void ReadStatus(Mcp25xxx mcp25xxx)
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

void RxStatus(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Rx Status Instruction");
    RxStatusResponse rxStatusResponse = mcp25xxx.RxStatus();
    Console.WriteLine($"Value: 0x{rxStatusResponse.ToByte():X2}");
    Console.WriteLine($"Filter Match: {rxStatusResponse.FilterMatch}");
    Console.WriteLine($"Message Type Received: {rxStatusResponse.MessageTypeReceived}");
    Console.WriteLine($"Received Message: {rxStatusResponse.ReceivedMessage}");
}

void BitModify(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Bit Modify Instruction");
    mcp25xxx.BitModify(Address.CanIntE, 0b1010_0110, 0b1111_1111);
}

void TransmitMessage(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Transmit Message");

    mcp25xxx.WriteByte(
        new CanCtrl(CanCtrl.PinPrescaler.ClockDivideBy8,
            false,
            false,
            false,
            OperationMode.NormalOperation));

    byte[] data = new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111, 0b1000_1001 };

    mcp25xxx.Write(
        Address.TxB0Sidh,
        new byte[]
        {
            new TxBxSidh(0, 0b0000_1001).ToByte(), new TxBxSidl(0, 0b001, false, 0b00).ToByte(),
            new TxBxEid8(0, 0b0000_0000).ToByte(), new TxBxEid0(0, 0b0000_0000).ToByte(),
            new TxBxDlc(0, data.Length, false).ToByte()
        });

    mcp25xxx.Write(Address.TxB0D0, data);

    // Send with TxB0 buffer.
    mcp25xxx.RequestToSend(true, false, false);
}

void LoopbackMode(Mcp25xxx mcp25xxx)
{
    Console.WriteLine("Loopback Mode");
    mcp25xxx.WriteByte(
        new CanCtrl(
            CanCtrl.PinPrescaler.ClockDivideBy8,
            true,
            false,
            false,
            OperationMode.Loopback));
}
