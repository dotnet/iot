// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    internal class SpiAdapter : IBusDevice
    {
        private SpiDevice _device;
        private int _deviceAddress;

        public SpiAdapter(SpiDevice device, int deviceAddress)
        {
            _device = device;
            _deviceAddress = deviceAddress;
        }

        public void Dispose() => _device?.Dispose();

        public void Read(byte registerAddress, Span<byte> buffer)
        {
            // Include OpCode and Register Address.
            Span<byte> writeBuffer = stackalloc byte[buffer.Length + 2];
            writeBuffer[0] = GetOpCode(_deviceAddress, isReadCommand: true);
            writeBuffer[0] = registerAddress;
            Span<byte> readBuffer = stackalloc byte[buffer.Length + 2];

            _device.TransferFullDuplex(writeBuffer, readBuffer);

            // First 2 bytes are from sending OpCode and Register Address.
            readBuffer.Slice(2).CopyTo(buffer);
        }

        public void Write(byte registerAddress, Span<byte> data)
        {
            // Include OpCode and Register Address.
            Span<byte> writeBuffer = stackalloc byte[data.Length + 2];
            writeBuffer[0] = GetOpCode(_deviceAddress, isReadCommand: false);
            writeBuffer[0] = registerAddress;
            data.CopyTo(writeBuffer.Slice(2));

            _device.Write(writeBuffer);
        }

        private static byte GetOpCode(int deviceAddress, bool isReadCommand)
        {
            int opCode = deviceAddress << 1;

            if (isReadCommand)
            {
                // Set read bit.
                opCode |= 0b000_0001;
            }

            return (byte)opCode;
        }
    }
}
