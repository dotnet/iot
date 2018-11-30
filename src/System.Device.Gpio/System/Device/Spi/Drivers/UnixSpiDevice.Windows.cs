// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi.Drivers
{
    public class UnixSpiDevice : SpiDevice
    {
        public UnixSpiDevice(SpiConnectionSettings settings) => throw new PlatformNotSupportedException($"The {GetType().Name} class is not available on Windows.");

        public override SpiConnectionSettings ConnectionSettings => throw new PlatformNotSupportedException();

        public override void Read(Span<byte> buffer) => throw new PlatformNotSupportedException();

        public override byte ReadByte() => throw new PlatformNotSupportedException();

        public override void Write(Span<byte> data) => throw new PlatformNotSupportedException();

        public override void WriteByte(byte data) => throw new PlatformNotSupportedException();
    }
}
