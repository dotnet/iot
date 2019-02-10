// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class Mcp23xxxTest
    {
        public static TheoryData<Mcp23xxx> TestDevices => new TheoryData<Mcp23xxx>
        {
            new Mcp16Mock(),
            new Mcp8Mock()
        };

        protected class Mcp16Mock : Mcp23x1x, IBusMock
        {
            public Mcp16Mock(int deviceAddress = 0x20)
                : base(new BusMock(ports: 2), deviceAddress)
            {
            }

            public BusMock BusMock => (BusMock)_device;
        }

        protected class Mcp8Mock : Mcp23x0x, IBusMock
        {
            public Mcp8Mock(int deviceAddress = 0x20)
                : base(new BusMock(ports: 1), deviceAddress)
            {
            }

            public BusMock BusMock => (BusMock)_device;
        }

        protected interface IBusMock
        {
            BusMock BusMock { get; }
        }

        protected class BusMock : IBusDevice
        {
            public BusMock(int ports)
            {
                // Default valid register count
                _registers = new byte[ports == 1 ? 0x0A + 1 : 0x15A + 1];
                _ports = ports;
            }

            private int _ports;
            private byte[] _lastReadBuffer;
            private byte[] _lastWriteBuffer;

            // OLATB address is 0x15
            private byte[] _registers;

            public byte LastReadRegister { get; private set; }
            public byte LastWriteRegister { get; private set; }
            public Span<byte> Registers => _registers;

            // Can't coalesce here https://github.com/dotnet/roslyn/issues/29927
            public ReadOnlySpan<byte> LastReadBuffer => _lastReadBuffer == null ? ReadOnlySpan<byte>.Empty : _lastReadBuffer;
            public ReadOnlySpan<byte> LastWriteBuffer => _lastWriteBuffer == null ? ReadOnlySpan<byte>.Empty : _lastWriteBuffer;

            public void Dispose()
            {
            }

            public void Read(byte registerAddress, Span<byte> buffer)
            {
                LastReadRegister = registerAddress;
                _lastReadBuffer = buffer.ToArray();

                // By default the Mcp chips shift registers sequentially
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = _registers[i + registerAddress];
                }
            }

            public void Write(byte registerAddress, Span<byte> data)
            {
                LastWriteRegister = registerAddress;
                _lastWriteBuffer = data.ToArray();

                // GPIO to OLAT (as GPIO writes change OLAT)
                // These are the address in default addressing mode (BANK0)
                if ((_ports == 2 && (registerAddress == 0x12 || registerAddress == 0x13))
                    || (_ports == 1 && (registerAddress == 0x09)))
                {
                    Write((byte)(registerAddress + _ports), data);
                }
                else
                {
                    // By default the Mcp chips shift registers sequentially
                    for (int i = 0; i < data.Length; i++)
                    {
                        _registers[i + registerAddress] = data[i];
                    }
                }
            }
        }
    }
}
