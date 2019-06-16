// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace System.Device.Gpio.System.Device.I2c
{
    public interface II2cController : IDisposable
    {
        /// <summary>
        /// The bus ID the I2C devices are connected to.
        /// </summary>
        int BusId { get; }

        /// <summary>
        /// Adds a new I2C device to controller.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to add.</param>
        void AddDevice(int address);

        /// <summary>
        /// Adds a new I2C device to controller.
        /// </summary>
        /// <param name="device">The I2C device to add.</param>
        void AddDevice(I2cDevice device);

        /// <summary>
        /// Removes specified I2C device from controller.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to remove.</param>
        void RemoveDevice(int address);

        /// <summary>
        /// Removes all I2C devices from controller.
        /// </summary>
        void ClearDevices();

        /// <summary>
        /// Reads data from the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        void Read(int address, Span<byte> buffer);

        /// <summary>
        /// Reads a byte from the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <returns>A byte read from the I2C device.</returns>
        byte ReadByte(int address);

        /// <summary>
        /// Writes data to the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        void Write(int address, ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Writes a byte to the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="value">The byte to be written to the I2C device.</param>
        void WriteByte(int address, byte value);

        /// <summary>
        /// Writes and reads data to the specified I2C device with a restart condition between operations.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to write and read.</param>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        void WriteRead(int address, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);
    }
}
