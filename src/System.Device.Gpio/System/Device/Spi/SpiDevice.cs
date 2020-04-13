// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// The communications channel to a device on a SPI bus.
    /// </summary>
    public abstract class SpiDevice : IDisposable
    {
        public SpiDevice(SpiConnectionSettings settings, Board board)
        {
            ConnectionSettings = settings;
            Board = board;
        }

        /// <summary>
        /// The connection settings of a device on a SPI bus.
        /// </summary>
        public SpiConnectionSettings ConnectionSettings
        {
            get;
        }

        public Board Board { get; }

        /// <summary>
        /// Reads a byte from the SPI device.
        /// </summary>
        /// <returns>A byte read from the SPI device.</returns>
        public abstract byte ReadByte();

        /// <summary>
        /// Reads data from the SPI device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the SPI device.
        /// The length of the buffer determines how much data to read from the SPI device.
        /// </param>
        public abstract void Read(Span<byte> buffer);

        /// <summary>
        /// Writes a byte to the SPI device.
        /// </summary>
        /// <param name="value">The byte to be written to the SPI device.</param>
        public abstract void WriteByte(byte value);

        /// <summary>
        /// Writes data to the SPI device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the SPI device.
        /// </param>
        public abstract void Write(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Writes and reads data from the SPI device.
        /// </summary>
        /// <param name="writeBuffer">The buffer that contains the data to be written to the SPI device.</param>
        /// <param name="readBuffer">The buffer to read the data from the SPI device.</param>
        public abstract void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);

        /// <summary>
        /// Creates a communications channel to a device on a SPI bus running on the current hardware
        /// </summary>
        /// <param name="settings">The connection settings of a device on a SPI bus.</param>
        /// <returns>A communications channel to a device on a SPI bus running on Windows 10 IoT.</returns>
        [Obsolete]
        public static SpiDevice Create(SpiConnectionSettings settings)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new Windows10SpiDevice(settings);
            }
            else
            {
                return new UnixSpiDevice(settings);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
