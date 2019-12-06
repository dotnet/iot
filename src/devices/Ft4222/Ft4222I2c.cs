// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Ft4222;
using System.IO;

namespace System.Device.I2c
{
    /// <summary>
    /// FT4222 I2C Device
    /// </summary>
    public class Ft4222I2c : I2cDevice
    {
        private I2cConnectionSettings _settings;
        private IntPtr _ftHandle = new IntPtr();

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public DeviceInformation DeviceInformation { get; internal set; }

        /// <summary>
        /// Create a FT4222 I2C Device
        /// </summary>
        /// <param name="settings">I2C Connection Settings</param>
        public Ft4222I2c(I2cConnectionSettings settings)
        {
            _settings = settings;
            // Check device
            var devInfos = FtCommon.GetDevices();
            // Select the one from bus Id
            DeviceInformation = devInfos[_settings.BusId];            
            
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, ref _ftHandle);

            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to open device {DeviceInformation.Description}, status: {ftStatus}");

            // Set the clock
            FtClockRate ft4222Clock = FtClockRate.Clock24MHz;

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed set clock rate {ft4222Clock} on device: {DeviceInformation.Description}, status: {ftStatus}");

            // Set the device as I2C Master
            ftStatus = FtFunction.FT4222_I2CMaster_Init(_ftHandle, 400);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to initialize I2C Master mode on device: {DeviceInformation.Description}, status: {ftStatus}");
        }

        /// <summary>
        /// The connection settings of a device on an I2C bus. The connection settings are immutable after the device is created
        /// so the object returned will be a clone of the settings object.
        /// </summary>
        public override I2cConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override void Read(Span<byte> buffer)
        {
            byte[] buff = new byte[buffer.Length];
            ushort byteRead = 0;
            var ftStatus = FtFunction.FT4222_I2CMaster_Read(_ftHandle, (ushort)_settings.DeviceAddress, buff, (ushort)buff.Length, ref byteRead);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");

            buff.CopyTo(buffer);
        }

        /// <summary>
        /// Reads a byte from the I2C device.
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public override byte ReadByte()
        {
            byte[] toRead = new byte[1];
            Read(toRead);
            return toRead[0];
        }

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ushort byteSent = 0;
            var ftStatus = FtFunction.FT4222_I2CMaster_Write(_ftHandle, (ushort)_settings.DeviceAddress, buffer.ToArray(), (ushort)buffer.Length, ref byteSent);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
        }

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public override void WriteByte(byte value)
        {
            Write(new byte[] { value });
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the I2C bus on which the device is connected, 
        /// and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(writeBuffer);
            Read(readBuffer);
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
