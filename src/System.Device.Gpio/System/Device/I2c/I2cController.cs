// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Device.I2c;

namespace System.Device.Gpio.System.Device.I2c
{
    public class I2cController : II2cController
    {
        private readonly Dictionary<int, I2cDevice> _devices;

        /// <summary>
        /// Initializes new instance of I2cController.
        /// </summary>
        /// <param name="busId">The bus ID the I2C device is connected to.</param>
        /// <param name="addresses">The bus addresses of initial I2C devices to add.</param>
        public I2cController(int busId, params int[] addresses)
        {
            BusId = busId;
            _devices = new Dictionary<int, I2cDevice>();

            if (addresses != null)
            {
                foreach (int address in addresses)
                {
                    AddDevice(address);
                }
            }
        }

        /// <summary>
        /// The bus ID the I2C devices are connected to.
        /// </summary>
        public int BusId { get; }

        /// <summary>
        /// Gets the I2C device for specified address.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to get.</param>
        /// <returns>The I2C device with specified address.</returns>
        private I2cDevice GetDevice(int address)
        {
            if (!_devices.TryGetValue(address, out I2cDevice device))
            {
                throw new InvalidOperationException("The specified address is not being used with an I2C device.");
            }

            return device;
        }

        /// <summary>
        /// Adds a new I2C device to controller.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to add.</param>
        public void AddDevice(int address)
        {
            if (_devices.ContainsKey(address))
            {
                throw new InvalidOperationException("The specified address is already used with another I2C device.");
            }

            _devices.Add(address, I2cDevice.CreateDevice(new I2cConnectionSettings(BusId, address)));
        }

        /// <summary>
        /// Adds a new I2C device to controller.
        /// </summary>
        /// <param name="device">The I2C device to add.</param>
        public void AddDevice(I2cDevice device)
        {
            int address = device.ConnectionSettings.DeviceAddress;

            if (device.ConnectionSettings.BusId != BusId)
            {
                throw new InvalidOperationException($"The device bus ID ({device.ConnectionSettings.BusId}) does not match the controller bus ID ({BusId}).");
            }

            // TODO: Should this compare type with other devices added to makes sure they are all the same.
            // Example: Would not want to add one for UnixI2cDevice and another for GpioI2cDevice.

            if (_devices.ContainsKey(address))
            {
                throw new InvalidOperationException("The specified address is already used with another I2C device.");
            }

            _devices.Add(address, device);
        }
        
        /// <summary>
        /// Removes specified I2C device from controller.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to remove.</param>
        public void RemoveDevice(int address)
        {
            GetDevice(address);  // This just checks if device with address is present before removing.
            _devices.Remove(address);
        }

        /// <summary>
        /// Removes all I2C devices from controller.
        /// </summary>
        public void ClearDevices() => _devices.Clear();

        /// <summary>
        /// Reads data from the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public void Read(int address, Span<byte> buffer) => GetDevice(address).Read(buffer);

        /// <summary>
        /// Reads a byte from the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to read.</param>
        /// <returns>A byte read from the I2C device.</returns>
        public byte ReadByte(int address) => GetDevice(address).ReadByte();

        /// <summary>
        /// Writes data to the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public void Write(int address, ReadOnlySpan<byte> buffer) => GetDevice(address).Write(buffer);

        /// <summary>
        /// Writes a byte to the specified I2C device.
        /// </summary>
        /// <param name="address">The bus address of the I2C device to write.</param>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public void WriteByte(int address, byte value) => GetDevice(address).WriteByte(value);

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
        public void WriteRead(int address, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) => GetDevice(address).WriteRead(writeBuffer, readBuffer);

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            foreach (KeyValuePair<int, I2cDevice> device in _devices)
            {
                device.Value.Dispose();
            }

            _devices.Clear();
        }
    }
}
