// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Devices.Enumeration;
using WinSpi = Windows.Devices.Spi;

namespace System.Device.Spi.Drivers
{
    public class Windows10SpiDevice : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;
        private WinSpi.SpiDevice _winDevice;

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
                throw new ArgumentException($"No SPI device exists for BusId {settings.BusId}", $"{nameof(settings)}.{nameof(settings.BusId)}");
            }

            _winDevice = WinSpi.SpiDevice.FromIdAsync(deviceInformationCollection[0].Id, winSettings).WaitForCompletion();
        }

        public override SpiConnectionSettings ConnectionSettings => _settings;

        public override byte ReadByte()
        {
            byte[] buffer = new byte[1];
            _winDevice.Read(buffer);
            return buffer[0];
        }

        public override void Read(Span<byte> buffer)
        {
            byte[] byteArray = new byte[buffer.Length];
            _winDevice.Read(byteArray);
            new Span<byte>(byteArray).CopyTo(buffer);
        }

        public override void WriteByte(byte data)
        {
            _winDevice.Write(new[] { data });
        }

        public override void Write(Span<byte> data)
        {
            _winDevice.Write(data.ToArray());
        }

        public override void TransferFullDuplex(Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (writeBuffer.Length != readBuffer.Length)
            {
                throw new ArgumentException($"Parameters '{nameof(writeBuffer)}' and '{nameof(readBuffer)}' must have the same length");
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
                    throw new ArgumentException($"SPI mode not supported: {mode}", nameof(mode));
            }
        }
    }
}
