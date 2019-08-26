// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        protected class SpiAdapter : BusAdapter
        {
            private SpiDevice _device;
            private int _deviceAddress;

            public SpiAdapter(SpiDevice device, int deviceAddress)
            {
                _device = device;
                _deviceAddress = deviceAddress;
            }

            public override void Dispose() => _device?.Dispose();

            public override void Read(byte registerAddress, Span<byte> buffer)
            {
                // Include OpCode and Register Address.
                Span<byte> writeBuffer = stackalloc byte[]
                {
                    GetOpCode(_deviceAddress, isReadCommand: true),
                    registerAddress
                };

                Span<byte> readBuffer = stackalloc byte[buffer.Length + 2];

                // Should this also contain the op code and register?
                // Why are we transferring full duplex if we only really
                // need to read?
                _device.TransferFullDuplex(writeBuffer, readBuffer);

                // First 2 bytes are from sending OpCode and Register Address.
                readBuffer.Slice(2).CopyTo(buffer);
            }

            public override void Write(byte registerAddress, Span<byte> data)
            {
                // Include OpCode and Register Address.
                Span<byte> writeBuffer = stackalloc byte[data.Length + 2];
                writeBuffer[0] = GetOpCode(_deviceAddress, isReadCommand: false);
                writeBuffer[1] = registerAddress;
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
}
