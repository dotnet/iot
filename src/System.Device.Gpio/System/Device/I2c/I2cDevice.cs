// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    public abstract class I2cDevice : IDisposable
    {
        public abstract I2cConnectionSettings ConnectionSettings { get; }
        public abstract byte ReadByte();
        public abstract void Read(Span<byte> buffer);
        public abstract void WriteByte(byte data);
        public abstract void Write(Span<byte> data);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
