// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Devices.I2c
{
    public abstract class I2cDevice : IDisposable
    {
        protected I2cConnectionSettings _settings;

        public I2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
        }

        public abstract void Dispose();

        public I2cConnectionSettings GetConnectionSettings() => new I2cConnectionSettings(_settings);

        public abstract void WriteRead(byte[] writeBuffer, byte[] readBuffer);

        public abstract void Read(byte[] buffer);
        public abstract byte Read8();
        public abstract ushort Read16();
        public abstract uint Read24();
        public abstract uint Read32();
        public abstract ulong Read64();

        public abstract void Write(params byte[] buffer);
        public abstract void Write8(byte value);
        public abstract void Write16(ushort value);
        public abstract void Write24(uint value);
        public abstract void Write32(uint value);
        public abstract void Write64(ulong value);
    }
}
