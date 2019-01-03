// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public abstract class Mcp23Sxx : Mcp23xxx
    {
        private readonly SpiDevice _spiDevice;
        
        public Mcp23Sxx(int deviceAddress, SpiDevice spiDevice, int? reset = null, int? intA = null, int? intB = null)
            : base(deviceAddress, reset, intA, intB)
        {
            _spiDevice = spiDevice;
        }

        public override byte Read(Register.Address registerAddress, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte[] readBuffer = Read(registerAddress, 1, port, bank);
            return readBuffer[0];
        }

        public override byte[] Read(Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byteCount += 2;  // Include OpCode and Register Address.
            byte[] writeBuffer = new byte[byteCount];
            writeBuffer[0] = OpCode.GetOpCode(DeviceAddress, true);
            writeBuffer[1] = Register.GetMappedAddress(startingRegisterAddress, port, bank);
            byte[] readBuffer = new byte[byteCount];

            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            return readBuffer.AsSpan().Slice(2).ToArray();  // First 2 bytes are from sending OpCode and Register Address.
        }

        public override void Write(Register.Address registerAddress, byte data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            Write(registerAddress, new byte[] { data }, port, bank);
        }

        public override void Write(Register.Address startingRegisterAddress, byte[] data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte[] writeBuffer = new byte[data.Length + 2]; // Include OpCode and Register Address.
            writeBuffer[0] = OpCode.GetOpCode(DeviceAddress, false);
            writeBuffer[1] = Register.GetMappedAddress(startingRegisterAddress, port, bank);
            data.CopyTo(writeBuffer, 2);

            _spiDevice.Write(writeBuffer);
        }

        public override void Dispose()
        {
            _spiDevice?.Dispose();
            base.Dispose();
        }
    }
}
