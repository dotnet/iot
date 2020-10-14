// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public partial class Mcp23s17
    {
        /// <summary>
        /// SPI adapter for Mcp23S17
        /// </summary>
        protected class Mcp23S17SpiAdapter : SpiAdapter
        {
            private SpiDevice _device;
            private int _deviceAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="Mcp23S17SpiAdapter"/> class.
            /// </summary>
            /// <param name="device">The SPI device used for communication.</param>
            /// <param name="deviceAddress">The device address for the connection on the SPI bus.</param>
            public Mcp23S17SpiAdapter(SpiDevice device, int deviceAddress)
                : base(device, deviceAddress)
            {
                _device = device;
                _deviceAddress = deviceAddress;
            }

            /// <inheritdoc/>
            public override void Read(byte registerAddress, Span<byte> buffer)
            {
                // Include OpCode and Register Address.
                Span<byte> writeBuffer = stackalloc byte[]
                {
                    GetOpCode(_deviceAddress, isReadCommand: true),
                    registerAddress
                };

                Span<byte> readBuffer = stackalloc byte[
                    writeBuffer.Length > buffer.Length
                        ? writeBuffer.Length
                        : buffer.Length
                ];

                // Should this also contain the op code and register?
                // Why are we transferring full duplex if we only really
                // need to read?
                _device.TransferFullDuplex(writeBuffer, readBuffer);

                // First 2 bytes are from sending OpCode and Register Address.
                readBuffer.Slice(2).CopyTo(buffer);
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
