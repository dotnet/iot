// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace System.Device.I2c
{
    public abstract class I2cDevice : IDisposable
    {
        public abstract I2cConnectionSettings ConnectionSettings { get; }
        public abstract byte ReadByte();
        public abstract void Read(Span<byte> buffer);
        public abstract void WriteByte(byte data);
        public abstract void Write(Span<byte> data);

        /// <summary>
        /// Initializes new instance of I2cDevice based on OS platform.
        /// </summary>
        /// <param name="busId">The bus ID the device is connected to.</param>
        /// <param name="deviceAddress">The bus address of the device.</param>
        public static I2cDevice CreateI2cDevice(int busId, int deviceAddress)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(busId, deviceAddress);
            OSPlatform osPlatform = Interop.OperatingSystem.GetOsPlatform();

            if (osPlatform == OSPlatform.Linux)
            {
                return new Drivers.UnixI2cDevice(settings);
            }
            else if (osPlatform == OSPlatform.Windows)
            {
                return new Drivers.Windows10I2cDevice(settings);
            }
            else
            {
                throw new PlatformNotSupportedException($"I2C not supported for this platform.");
            }
        }

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
