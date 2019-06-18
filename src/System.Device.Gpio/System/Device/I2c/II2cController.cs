// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    public interface II2cController : IDisposable
    {
        
        /// <summary>
        /// Opens an I2C device with specified settings in order for it to be ready to use with the controller.
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        void OpenDevice(I2cConnectionSettings settings);

        /// <summary>
        /// Opens an I2C device in order for it to be ready to use with the controller.
        /// </summary>
        /// <param name="device">The I2C device to open.</param>
        void OpenDevice(I2cDevice device);

        /// <summary>
        /// Closes an open I2C device based on specified settings.
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        void CloseDevice(I2cConnectionSettings settings);

        /// <summary>
        /// Closes an open I2C device.
        /// </summary>
        /// <param name="device">The I2C device to close.</param>
        void CloseDevice(I2cDevice device);

        /// <summary>
        /// Removes all I2C devices from controller.
        /// </summary>
        void ClearDevices();

        /// <summary>
        /// Gets the I2C device associated with the specified bus ID and address.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="device">
        /// When this method returns, contains the I2C device associated with the specified bus ID and address, if they are found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        bool TryGetDevice(int busId, int address, out I2cDevice? device);

        /// <summary>
        /// Reads data from the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        void Read(int busId, int address, Span<byte> buffer);

        /// <summary>
        /// Reads a byte from the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <returns>A byte read from the I2C device.</returns>
        byte ReadByte(int busId, int address);

        /// <summary>
        /// Writes data to the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        void Write(int busId, int address, ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Writes a byte to the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="value">The byte to be written to the I2C device.</param>
        void WriteByte(int busId, int address, byte value);

        /// <summary>
        /// Writes and reads data to the specified I2C device with a restart condition between operations.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write and read.</param>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        void WriteRead(int busId, int address, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);
    }
}
