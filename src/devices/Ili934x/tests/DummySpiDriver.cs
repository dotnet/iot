// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ili934x.Tests
{
    /// <summary>
    /// An SPI driver that does nothing, except store the output data in a buffer for testing.
    /// Useful for testing (the default SpiDevice is not mockable)
    /// </summary>
    internal class DummySpiDriver : SpiDevice
    {
        public DummySpiDriver()
        {
            ConnectionSettings = new SpiConnectionSettings(0, 1);
        }

        public override SpiConnectionSettings ConnectionSettings { get; }

        public List<byte> Data { get; } = new List<byte>();

        public override void Read(Span<byte> buffer)
        {
            buffer.Clear();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Data.AddRange(buffer.ToArray());
        }

        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Data.AddRange(writeBuffer.ToArray());
            readBuffer.Clear();
        }
    }
}
