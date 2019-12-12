// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 I2C Device
    /// </summary>
    public class Ft4222I2c : I2cDevice
    {
        const uint I2cMasterFrequencyKbps = 400;

        private I2cConnectionSettings _settings;
        private SafeFtHandle _ftHandle;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public DeviceInformation DeviceInformation { get; private set; }

        /// <summary>
        /// Create a FT4222 I2C Device
        /// </summary>
        /// <param name="settings">I2C Connection Settings</param>
        public Ft4222I2c(I2cConnectionSettings settings)
        {
            _settings = settings;
            // Check device
            var devInfos = FtCommon.GetDevices();
            if (devInfos.Count == 0)
                throw new IOException("No FTDI device available");

            // Select the one from bus Id
            // FT4222 propose depending on the mode multiple interfaces. Only the A is available for I2C or where there is none as it's the only interface
            var devInfo = devInfos.Where(m => m.Description == "FT4222 A" || m.Description == "FT4222").ToArray();
            if ((devInfo.Length == 0) || (devInfo.Length < _settings.BusId))
                throw new IOException($"Can't find a device to open I2C on index {_settings.BusId}");

            DeviceInformation = devInfo[_settings.BusId];
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, out _ftHandle);

            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to open device {DeviceInformation.Description}, status: {ftStatus}");

            // Set the clock
            FtClockRate ft4222Clock = FtClockRate.Clock24MHz;

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed set clock rate {ft4222Clock} on device: {DeviceInformation.Description}, status: {ftStatus}");

            // Set the device as I2C Master
            ftStatus = FtFunction.FT4222_I2CMaster_Init(_ftHandle, I2cMasterFrequencyKbps);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to initialize I2C Master mode on device: {DeviceInformation.Description}, status: {ftStatus}");
        }

        /// <inheritdoc/>
        public override I2cConnectionSettings ConnectionSettings => _settings;

        /// <inheritdoc/>
        public override void Read(Span<byte> buffer)
        {
            ushort byteRead;
            var ftStatus = FtFunction.FT4222_I2CMaster_Read(_ftHandle, (ushort)_settings.DeviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out byteRead);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");
        }

        /// <inheritdoc/>
        public override byte ReadByte()
        {
            Span<byte> toRead = stackalloc byte[1];
            Read(toRead);
            return toRead[0];
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ushort byteSent;
            var ftStatus = FtFunction.FT4222_I2CMaster_Write(_ftHandle, (ushort)_settings.DeviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out byteSent);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            Span<byte> toWrite = stackalloc byte[1] { value };
            Write(toWrite);
        }

        /// <inheritdoc/>
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(writeBuffer);
            Read(readBuffer);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ftHandle.Dispose();
            base.Dispose(disposing);
        }
    }
}
