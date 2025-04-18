// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Tca955x.Tests
{
    /// <summary>
    /// This class allows mocking of the Read/Write functions of I2cDevice taking Spans
    /// </summary>
    public abstract class MockableI2cDevice : I2cDevice
    {
        /// <summary>
        /// These are mockable, operations taking Span&lt;T&gt; are not
        /// </summary>
        /// <param name="data"></param>
        public abstract void Read(byte[] data);
        public sealed override void Read(Span<byte> buffer)
        {
            byte[] b = new byte[buffer.Length];
            Read(b);
            b.CopyTo(buffer);
        }

        public abstract void Write(byte[] data);

        public sealed override void Write(ReadOnlySpan<byte> buffer)
        {
            byte[] data = new byte[buffer.Length];
            buffer.CopyTo(data);
            Write(data);
        }

        public sealed override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(writeBuffer);
            Read(readBuffer);
        }
    }
}
