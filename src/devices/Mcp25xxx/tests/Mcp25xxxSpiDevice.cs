// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;

namespace Iot.Device.Mcp25xxx.Tests
{
    public class Mcp25xxxSpiDevice : SpiDevice
    {
        public event Action? TransferCompleted;

        public override SpiConnectionSettings ConnectionSettings => throw new NotImplementedException();
        public byte[]? NextReadBuffer { get; set; }

        public byte NextReadByte { get; set; }

        public byte[]? LastWriteBuffer { get; private set; }

        public byte LastWriteByte { get; private set; }

        public override void Read(Span<byte> buffer)
        {
            NextReadBuffer = buffer.ToArray();
        }

        public override byte ReadByte() => NextReadByte;

        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            LastWriteBuffer = writeBuffer.ToArray();
            NextReadBuffer.CopyTo(readBuffer);
            TransferCompleted?.Invoke();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            LastWriteBuffer = buffer.ToArray();
        }

        public override void WriteByte(byte value)
        {
            LastWriteByte = value;
        }
    }
}
