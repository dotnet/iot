// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 device information
    /// </summary>
    public class FtDevice
    {
        /// <summary>
        /// Instantiates a DeviceInformation object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public FtDevice(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        {
            Flags = flags;
            Type = type;
            Id = id;
            LocId = locId;
            SerialNumber = serialNumber;
            Description = description;
        }

        /// <summary>
        /// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
        /// </summary>
        public FtFlag Flags { get; set; }

        /// <summary>
        /// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        public FtDeviceType Type { get; set; }

        /// <summary>
        /// The Vendor ID and Product ID of the device.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The physical location identifier of the device.
        /// </summary>
        public uint LocId { get; set; }

        /// <summary>
        /// The device serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// The device description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates I2C bus related to this device
        /// </summary>
        /// <returns>I2cBus instance</returns>
        public I2cBus CreateI2cBus()
        {
            return new Ft4222I2cBus(this);
        }
    }
}
