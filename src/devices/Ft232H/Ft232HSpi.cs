// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Linq;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// SPI Device for FT232H
    /// </summary>
    public class Ft232HSpi : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;

        /// <inheritdoc/>
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ftx232HDevice DeviceInformation { get; internal set; }

        internal Ft232HSpi(SpiConnectionSettings settings, Ftx232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            _settings = settings;
            if ((_settings.ChipSelectLine < 3) || (_settings.ChipSelectLine > DeviceInformation.PinCount))
            {
                throw new ArgumentException($"Chip Select line has to be between 3 and {DeviceInformation.PinCount - 1}");
            }

            if (DeviceInformation.ConnectionSettings.Where(m => m.ChipSelectLine == _settings.ChipSelectLine).Any())
            {
                throw new ArgumentException("Chip Select already in use");
            }

            // Open the device
            DeviceInformation.ConnectionSettings.Add(_settings);
            DeviceInformation.SpiInitialize();
        }

        /// <inheritdoc/>
        public override void Read(Span<byte> buffer)
        {
            DeviceInformation.SpiRead(_settings, buffer);
        }

        /// <inheritdoc/>
        public override byte ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            DeviceInformation.SpiRead(_settings, buffer);
            return buffer[0];
        }

        /// <inheritdoc/>
        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            DeviceInformation.SpiWriteRead(_settings, writeBuffer, readBuffer);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            DeviceInformation.SpiWrite(_settings, buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            DeviceInformation.SpiWrite(_settings, stackalloc byte[1] { value });
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            DeviceInformation.ConnectionSettings.Remove(_settings);
            DeviceInformation.SpiDeinitialize();
            base.Dispose(disposing);
        }
    }
}
