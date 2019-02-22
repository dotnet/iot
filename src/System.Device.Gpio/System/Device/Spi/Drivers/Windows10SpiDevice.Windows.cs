// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Devices.Enumeration;
using WinSpi = Windows.Devices.Spi;

namespace System.Device.Spi.Drivers
{
    /// <summary>
    /// Represents an SPI communication channel running on Windows 10 IoT.
    /// </summary>
    public class Windows10SpiDevice : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;
        private WinSpi.SpiDevice _winDevice;

        /// <summary>
        /// Initializes new instance of Windows10SpiDevice that will use the specified settings to communicate with the SPI device.
        /// </summary>
        /// <param name="settings">
        /// The connection settings of a device on a SPI bus.
        /// </param>
        public Windows10SpiDevice(SpiConnectionSettings settings)
        {
            _settings = settings;
            var winSettings = new WinSpi.SpiConnectionSettings(_settings.ChipSelectLine)
            {
                Mode = ToWinMode(settings.Mode),
                DataBitLength = settings.DataBitLength,
                ClockFrequency = settings.ClockFrequency,
            };

            string busFriendlyName = $"SPI{settings.BusId}";
            string deviceSelector = WinSpi.SpiDevice.GetDeviceSelector(busFriendlyName);

            DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).WaitForCompletion();
            if (deviceInformationCollection.Count == 0)
            {
                throw new ArgumentException($"No SPI device exists for bus ID {settings.BusId}.", $"{nameof(settings)}.{nameof(settings.BusId)}");
            }

            _winDevice = WinSpi.SpiDevice.FromIdAsync(deviceInformationCollection[0].Id, winSettings).WaitForCompletion();
        }

        /// <summary>
        /// The connection settings of a device on a SPI bus.
        /// </summary>
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Reads a byte from the SPI device.
        /// </summary>
        /// <returns>A byte read from the SPI device.</returns>
        public override byte ReadByte()
        {
            byte[] buffer = new byte[1];
            _winDevice.Read(buffer);
            return buffer[0];
        }

        /// <summary>
        /// Reads data from the SPI device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the SPI device.
        /// The length of the buffer determines how much data to read from the SPI device.
        /// </param>
        public override void Read(Span<byte> buffer)
        {
            byte[] byteArray = new byte[buffer.Length];
            _winDevice.Read(byteArray);
            new Span<byte>(byteArray).CopyTo(buffer);
        }

        /// <summary>
        /// Writes a byte to the SPI device.
        /// </summary>
        /// <param name="value">The byte to be written to the SPI device.</param>
        public override void WriteByte(byte value)
        {
            _winDevice.Write(new[] { value });
        }

        /// <summary>
        /// Writes data to the SPI device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the SPI device.
        /// </param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _winDevice.Write(buffer.ToArray());
        }

        /// <summary>
        /// Writes and reads data from the SPI device.
        /// </summary>
        /// <param name="writeBuffer">The buffer that contains the data to be written to the SPI device.</param>
        /// <param name="readBuffer">The buffer to read the data from the SPI device.</param>
        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (writeBuffer.Length != readBuffer.Length)
            {
                throw new ArgumentException($"Parameters '{nameof(writeBuffer)}' and '{nameof(readBuffer)}' must have the same length.");
            }
            byte[] byteArray = new byte[readBuffer.Length];
            _winDevice.TransferFullDuplex(writeBuffer.ToArray(), byteArray);
        }

        public override void Dispose(bool disposing)
        {
            _winDevice?.Dispose();
            _winDevice = null;

            base.Dispose(disposing);
        }

        private static WinSpi.SpiMode ToWinMode(SpiMode mode)
        {
            switch (mode)
            {
                case SpiMode.Mode0:
                    return WinSpi.SpiMode.Mode0;
                case SpiMode.Mode1:
                    return WinSpi.SpiMode.Mode1;
                case SpiMode.Mode2:
                    return WinSpi.SpiMode.Mode2;
                case SpiMode.Mode3:
                    return WinSpi.SpiMode.Mode3;
                default:
                    throw new ArgumentException($"SPI mode {mode} not supported.", nameof(mode));
            }
        }
    }
}
