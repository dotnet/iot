// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23xxx : IDisposable
    {
        private GpioController _controller;
        private readonly SpiDevice _spiDevice;

        private enum CommunicationProtocol
        {
            I2c,
            I2cGpio,
            Spi,
            SpiGpio
        }

        public Mcp23xxx(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }

        public byte Read(int deviceAddress, Register.Address registerAddress, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte opCode = OpCode.GetOpCode(deviceAddress, true);
            byte mappedAddress = Register.GetMappedAddress(registerAddress, port, bank);
            byte[] writeBuffer = new byte[] { opCode, mappedAddress, 0 };
            byte[] readBuffer = new byte[3];

            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            return readBuffer[2];
        }

        public byte[] Read(int deviceAddress, Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte opCode = OpCode.GetOpCode(deviceAddress, true);
            byte mappedAddress = Register.GetMappedAddress(startingRegisterAddress, port, bank);

            byteCount += 2;  // Include OpCode and Register Address.
            byte[] writeBuffer = new byte[byteCount];
            writeBuffer[0] = opCode;
            writeBuffer[1] = (byte)startingRegisterAddress;
            byte[] readBuffer = new byte[byteCount];

            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            return readBuffer.AsSpan().Slice(2).ToArray();  // First 2 bytes are from sending OpCode and Register Address.
        }

        public void Write(int deviceAddress, Register.Address registerAddress, byte data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte opCode = OpCode.GetOpCode(deviceAddress, false);
            byte mappedAddress = Register.GetMappedAddress(registerAddress, port, bank);
            byte[] writeBuffer = new byte[] { opCode, mappedAddress, data };

            _spiDevice.Write(writeBuffer);
        }

        public void Write(int deviceAddress, Register.Address startingRegisterAddress, byte[] data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte opCode = OpCode.GetOpCode(deviceAddress, false);
            byte mappedAddress = Register.GetMappedAddress(startingRegisterAddress, port, bank);

            byte[] writeBuffer = new byte[data.Length + 2]; // Include OpCode and Register Address.
            writeBuffer[0] = opCode;
            writeBuffer[1] = (byte)startingRegisterAddress;
            data.CopyTo(writeBuffer, 2);

            _spiDevice.Write(writeBuffer);
        }
    }
}
