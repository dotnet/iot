// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Text;

namespace Iot.Device.Board.Tests
{
    internal sealed class SpiDummyDevice : SpiDevice
    {
        private bool _disposed;
        public SpiDummyDevice(SpiConnectionSettings connectionSettings, int[] pins)
        {
            ConnectionSettings = connectionSettings;
            Pins = pins;
            _disposed = false;
        }

        public override SpiConnectionSettings ConnectionSettings { get; }
        public int[] Pins { get; }

        public override byte ReadByte()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SpiDummyDevice));
            }

            return 0xF8;
        }

        public override void Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
