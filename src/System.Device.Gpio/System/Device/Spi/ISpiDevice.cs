// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    public interface ISpiDevice : IDisposable
    {
        SpiConnectionSettings GetConnectionSettings();
        void Read(byte[] buffer, int index, int count);
        byte ReadByte();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        void Write(byte[] buffer, int index, int count);
        void WriteByte(byte value);
        void WriteUInt16(ushort value);
        void WriteUInt32(uint value);
        void WriteUInt64(ulong value);
    }
}
