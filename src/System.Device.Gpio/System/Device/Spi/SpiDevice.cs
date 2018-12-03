// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    public abstract class SpiDevice : IDisposable
    {
        public abstract SpiConnectionSettings ConnectionSettings { get; }
        public abstract byte ReadByte();
        public abstract void Read(Span<byte> buffer);
        public abstract void WriteByte(byte data);
        public abstract void Write(Span<byte> data);
        public abstract void TransferFullDuplex(Span<byte> writeBuffer, Span<byte> readBuffer);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual void Dispose(bool disposing)
        {
            //Nothing to do in base class.
        }
    }
}
