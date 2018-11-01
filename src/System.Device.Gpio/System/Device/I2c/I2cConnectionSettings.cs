// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.I2c
{
    /// <summary>
    /// Class that holds the connection settings of a device on a I2c bus.
    /// </summary>
    public sealed class I2cConnectionSettings
    {
        private I2cConnectionSettings() { }

        /// <summary>
        /// Default constructor. Takes the bus id and the device address.
        /// </summary>
        /// <param name="busId">The bus in which the device will be connected to.</param>
        /// <param name="deviceAddress">The I2c address of the device.</param>
        public I2cConnectionSettings(uint busId, uint deviceAddress)
        {
            BusId = busId;
            DeviceAddress = deviceAddress;
        }

        /// <summary>
        /// The bus in which the device will be connected to.
        /// </summary>
        public uint BusId { get; }
        /// <summary>
        /// The I2c address of the device.
        /// </summary>
        public uint DeviceAddress { get; }
    }
}
