// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

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

        /// <summary>
        /// Initializes new instance of SpiDevice based on OS platform.
        /// </summary>
        /// <param name="busId">The bus ID the device is connected to.</param>
        /// <param name="chipSelectLine">The chip select line used on the bus.</param>
        public static SpiDevice CreateSpiDevice(int busId, int chipSelectLine)
        {
            SpiConnectionSettings settings = new SpiConnectionSettings(busId, chipSelectLine);
            OSPlatform osPlatform = Interop.OperatingSystem.GetOsPlatform();

            if (osPlatform == OSPlatform.Linux)
            {
                return new Drivers.UnixSpiDevice(settings);
            }
            else if (osPlatform == OSPlatform.Windows)
            {
                return new Drivers.Windows10SpiDevice(settings);
            }
            else
            {
                throw new PlatformNotSupportedException($"SPI not supported for this platform.");
            }
        }

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
