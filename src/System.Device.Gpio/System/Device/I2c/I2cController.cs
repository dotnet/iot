// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Device.I2c
{
    public sealed partial class I2cController : II2cController
    {
        private readonly Dictionary<(int, int), I2cDevice> _devices;

        /// <summary>
        /// Initializes new instance of I2cController.
        /// </summary>
        public I2cController()
        {
            _devices = new Dictionary<(int, int), I2cDevice>();
        }

        /// <summary>
        /// Gets the I2C device for specified address.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to get.</param>
        /// <returns>The I2C device with specified address.</returns>
        private I2cDevice GetDevice(int busId, int address)
        {
            if (!_devices.TryGetValue((busId, address), out I2cDevice device))
            {
                throw new InvalidOperationException("The specified bus ID and address is not being used with an I2C device.");
            }

            return device;
        }

        /// <summary>
        /// Opens an I2C device with specified settings in order for it to be ready to use with the controller.
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        public void OpenDevice(I2cConnectionSettings settings)
        {
            int busId = settings.BusId;
            int address = settings.DeviceAddress;

            if (_devices.ContainsKey((busId, address)))
            {
                throw new InvalidOperationException($"The specified bus ID ({busId}) and address ({address}) is already used with another I2C device.");
            }

            _devices.Add((busId, address), I2cController.Create(new I2cConnectionSettings(busId, address)));
        }

        /// <summary>
        /// Opens an I2C device in order for it to be ready to use with the controller.
        /// </summary>
        /// <param name="device">The I2C device to open.</param>
        public void OpenDevice(I2cDevice device)
        {
            // TODO: Should this compare type with other devices added to makes sure they are all the same (at least on same bus).
            // Example: Would not want to add one for UnixI2cDevice and another for GpioI2cDevice.

            OpenDevice(device.ConnectionSettings);
        }

        /// <summary>
        /// Closes an open I2C device based on specified settings.
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        public void CloseDevice(I2cConnectionSettings settings)
        {
            int busId = settings.BusId;
            int address = settings.DeviceAddress;

            GetDevice(busId, address);  // This checks if device with address is present before removing.
            _devices.Remove((busId, address));

        }

        /// <summary>
        /// Closes an open I2C device.
        /// </summary>
        /// <param name="device">The I2C device to close.</param>
        public void CloseDevice(I2cDevice device) => CloseDevice(device.ConnectionSettings);

        /// <summary>
        /// Removes all I2C devices from controller.
        /// </summary>
        public void ClearDevices() => _devices.Clear();

        /// <summary>
        /// Reads data from the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public void Read(int busId, int address, Span<byte> buffer) => GetDevice(busId, address).Read(buffer);

        /// <summary>
        /// Reads a byte from the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <returns>A byte read from the I2C device.</returns>
        public byte ReadByte(int busId, int address) => GetDevice(busId, address).ReadByte();

        /// <summary>
        /// Writes data to the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public void Write(int busId, int address, ReadOnlySpan<byte> buffer) => GetDevice(busId, address).Write(buffer);

        /// <summary>
        /// Writes a byte to the specified I2C device.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public void WriteByte(int busId, int address, byte value) => GetDevice(busId, address).WriteByte(value);

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
        public void WriteRead(int busId, int address, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) => GetDevice(busId, address).WriteRead(writeBuffer, readBuffer);

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            foreach (KeyValuePair<(int, int), I2cDevice> device in _devices)
            {
                device.Value.Dispose();
            }

            _devices.Clear();
        }
    }
}
