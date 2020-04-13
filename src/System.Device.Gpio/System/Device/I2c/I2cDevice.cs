// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    /// <summary>
    /// The communications channel to a device on an I2C bus.
    /// </summary>
    public abstract class I2cDevice : IDisposable
    {
        private I2cConnectionSettings _settings;

        [Obsolete]
        protected I2cDevice()
        {
        }

        protected I2cDevice(I2cConnectionSettings settings, Board board)
        {
            _settings = settings;
            Board = board;
            if (Board != null)
            {
                // Todo: if the second fails, the other stays reserved
                Board.ReservePin(Board.ConvertLogicalNumberingSchemeToPinNumber(settings.SdaPin), PinUsage.I2c, this);
                Board.ReservePin(Board.ConvertLogicalNumberingSchemeToPinNumber(settings.SclPin), PinUsage.I2c, this);
            }
        }

        public Board Board
        {
            get;
            private set;
        }

        /// <summary>
        /// The connection settings of a device on an I2C bus. The connection settings are immutable after the device is created
        /// so the object returned will be a clone of the settings object.
        /// </summary>
        public I2cConnectionSettings ConnectionSettings
        {
            get
            {
                return new I2cConnectionSettings(_settings);
            }
        }

        /// <summary>
        /// Reads a byte from the I2C device.
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public abstract byte ReadByte();

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public abstract void Read(Span<byte> buffer);

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public abstract void WriteByte(byte value);

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public abstract void Write(ReadOnlySpan<byte> buffer);

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
        public abstract void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);

        /// <summary>
        /// Creates a communications channel to a device on an I2C bus running on the current platform
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        /// <returns>A communications channel to a device on an I2C bus running on Windows 10 IoT.</returns>
        [Obsolete("Use Board.CreateI2cDevice() instead")]
        public static I2cDevice Create(I2cConnectionSettings settings)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new Windows10I2cDevice(settings);
            }
            else
            {
                return new UnixI2cDevice(settings);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Board != null)
                {
                    Board.ReleasePin(ConnectionSettings.SdaPin, PinUsage.I2c, this);
                    Board.ReleasePin(ConnectionSettings.SclPin, PinUsage.I2c, this);
                    Board = null; // Because the above must not happen twice
                }
            }
        }
    }
}
