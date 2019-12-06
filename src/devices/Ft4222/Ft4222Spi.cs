// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Ft4222;
using System.IO;

namespace System.Device.Spi
{
    /// <summary>
    /// Create a SPI Device based on FT4222 chipset
    /// </summary>
    public class Ft4222Spi : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;
        private IntPtr _ftHandle = new IntPtr();

        /// <summary>
        /// The connection settings of a device on a SPI bus. The connection settings are immutable after the device is created
        /// so the object returned will be a clone of the settings object.
        /// </summary>
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public DeviceInformation DeviceInformation { get; internal set; }

        /// <summary>
        /// Create an SPI FT4222 class
        /// </summary>
        /// <param name="settings">SPI Connection Settings</param>
        public Ft4222Spi(SpiConnectionSettings settings)
        {
            _settings = settings;
            // Check device
            var devInfos = FtCommon.GetDevices();
            // Select the one from bus Id
            DeviceInformation = devInfos[_settings.BusId];

            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, ref _ftHandle);

            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to open device {DeviceInformation.Description}with error: {ftStatus}");

            // Set the clock but we need some math
            var (ft4222Clock, tfSpiDiv) = CalculateBestClockRate();

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed set clock rate {ft4222Clock} on device: {DeviceInformation.Description}with error: {ftStatus}");


            SpiClockPolarity pol = SpiClockPolarity.ClockIdleLow;
            if ((_settings.Mode == SpiMode.Mode2) || (_settings.Mode == SpiMode.Mode3))
                pol = SpiClockPolarity.ClockIdelHigh;

            SpiClockPhase pha = SpiClockPhase.ClockLeading;
            if ((_settings.Mode == SpiMode.Mode1) || (_settings.Mode == SpiMode.Mode3))
                pha = SpiClockPhase.ClockTailing;

            // Configure the SPI             
            ftStatus = FtFunction.FT4222_SPIMaster_Init(_ftHandle, SpiOperatingMode.Single, tfSpiDiv, pol, pha, (byte)_settings.ChipSelectLine);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed setup SPI on device: {DeviceInformation.Description} with error: {ftStatus}");
        }

        private (FtClockRate clk, SpiClock spiClk) CalculateBestClockRate()
        {
            // Maximum is the System Clock / 1 = 80 MHz
            // Minimum is the System Clock / 512 = 24 / 256 = 93.75 KHz
            // Always take the below frequency to avoid over clocking
            if (_settings.ClockFrequency < 187500)
                return (FtClockRate.Clock24MHz, SpiClock.DivideBy256);
            if (_settings.ClockFrequency < 234375)
                return (FtClockRate.Clock24MHz, SpiClock.DivideBy256);
            if (_settings.ClockFrequency < 312500)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy256);
            if (_settings.ClockFrequency < 375000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy256);
            if (_settings.ClockFrequency < 468750)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy128);
            if (_settings.ClockFrequency < 625000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy128);
            if (_settings.ClockFrequency < 750000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy128);
            if (_settings.ClockFrequency < 937500)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy64);
            if (_settings.ClockFrequency < 1250000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy64);
            if (_settings.ClockFrequency < 1500000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy64);
            if (_settings.ClockFrequency < 1875000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy32);
            if (_settings.ClockFrequency < 2500000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy32);
            if (_settings.ClockFrequency < 3000000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy32);
            if (_settings.ClockFrequency < 3750000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy16);
            if (_settings.ClockFrequency < 5000000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy16);
            if (_settings.ClockFrequency < 6000000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy16);
            if (_settings.ClockFrequency < 7500000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy8);
            if (_settings.ClockFrequency < 10000000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy8);
            if (_settings.ClockFrequency < 12000000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy8);
            if (_settings.ClockFrequency < 15000000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy4);
            if (_settings.ClockFrequency < 20000000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy4);
            if (_settings.ClockFrequency < 24000000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy4);
            if (_settings.ClockFrequency < 30000000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy2);
            if (_settings.ClockFrequency < 40000000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy2);
            if (_settings.ClockFrequency < 48000000)
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy2);
            if (_settings.ClockFrequency < 60000000)
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy1);
            if (_settings.ClockFrequency < 80000000)
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy1);
            // Anything else will be 80 MHz
            return (FtClockRate.Clock80MHz, SpiClock.DivideBy1);
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
            byte[] readBuff = new byte[buffer.Length];
            ushort readBytes = 0;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleRead(_ftHandle, readBuff, (ushort)buffer.Length, ref readBytes, true);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");

            readBuff.CopyTo(buffer);
        }

        /// <summary>
        /// Reads a byte from the SPI device.
        /// </summary>
        /// <returns>A byte read from the SPI device.</returns>
        public override byte ReadByte()
        {
            byte[] toRead = new byte[1];
            Read(toRead);
            return toRead[1];
        }

        /// <summary>
        /// Writes and reads data from the SPI device.
        /// </summary>
        /// <param name="writeBuffer">The buffer that contains the data to be written to the SPI device.</param>
        /// <param name="readBuffer">The buffer to read the data from the SPI device.</param>
        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            byte[] readBuff = new byte[readBuffer.Length];
            ushort readBytes = 0;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleReadWrite(_ftHandle, readBuff, writeBuffer.ToArray(), (ushort)writeBuffer.Length, ref readBytes, true);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(TransferFullDuplex)} failed to do a full duplex transfer, error: {ftStatus}");

            readBuff.CopyTo(readBuffer);
        }

        /// <summary>
        /// Writes data to the SPI device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the SPI device.
        /// </param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ushort writeBytes = 0;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleWrite(_ftHandle, buffer.ToArray(), (ushort)buffer.Length, ref writeBytes, true);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
        }

        /// <summary>
        /// Writes a byte to the SPI device.
        /// </summary>
        /// <param name="value">The byte to be written to the SPI device.</param>
        public override void WriteByte(byte value)
        {
            Write(new byte[] { value });
        }

        /// <summary>
        /// Dispose the class
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_ftHandle != IntPtr.Zero)
            {
                FtFunction.FT4222_UnInitialize(_ftHandle);
                FtFunction.FT_Close(DeviceInformation.FtHandle);
            }

            base.Dispose(disposing);
        }
    }
}
