// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Common.Defnitions;

namespace Iot.Device.Vcnl4040.Tests
{
    internal class Vcnl4040TestDevice : I2cDevice
    {
        public byte[,] Data { get; set; } = new byte[0x0c, 0x02];

        public override I2cConnectionSettings ConnectionSettings => throw new NotImplementedException();

        public int GetData(CommandCode addr)
        {
            return Data[(byte)addr, 0] | Data[(byte)addr, 1] << 8;
        }

        public byte GetLsb(CommandCode addr) => Data[(byte)addr, 0];
        public byte GetMsb(CommandCode addr) => Data[(byte)addr, 1];

        public void SetData(CommandCode addr, byte lsb, byte msb)
        {
            Data[(byte)addr, 0] = lsb;
            Data[(byte)addr, 1] = msb;
        }

        public void SetData(CommandCode addr, int data)
        {
            SetLsb(addr, (byte)data);
            SetMsb(addr, (byte)(data >> 8));
        }

        public void SetLsb(CommandCode addr, byte lsb) => Data[(byte)addr, 0] = lsb;

        public void SetMsb(CommandCode addr, byte msb) => Data[(byte)addr, 1] = msb;

        public override void Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != 3)
            {
                throw new Exception("Write buffer with address, LSB and MSB expected)");
            }

            int addr = buffer[0];
            Data[addr, 0] = buffer[1];
            Data[addr, 1] = buffer[2];
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (writeBuffer.Length != 1)
            {
                throw new Exception("Write buffer with address (only)");
            }

            if (readBuffer.Length != 2)
            {
                throw new Exception("Read buffer for taking LSB and MSB from addressed location expected");
            }

            int addr = writeBuffer[0];
            readBuffer[0] = Data[addr, 0];
            readBuffer[1] = Data[addr, 1];
        }
    }
}
