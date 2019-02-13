// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c.Drivers
{
    public class UnixI2cDevice : I2cDevice
    {
        public UnixI2cDevice(I2cConnectionSettings settings) => throw new PlatformNotSupportedException($"The {GetType().Name} class is not available on Windows.");

        public string DevicePath { get; set; }

        public override I2cConnectionSettings ConnectionSettings => throw new PlatformNotSupportedException();

        public override void Read(Span<byte> buffer) => throw new PlatformNotSupportedException();

        public override byte ReadByte() => throw new PlatformNotSupportedException();

        public override void Write(Span<byte> data) => throw new PlatformNotSupportedException();

        public override void WriteByte(byte data) => throw new PlatformNotSupportedException();
    }
}
