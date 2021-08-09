// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 device information
    /// </summary>
    public class Ft4222Device : FtDevice
    {
        /// <summary>
        /// Instantiates a FT4222 Device object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public Ft4222Device(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        : base(flags, type, id, locId, serialNumber, description)
        {
        }

        /// <summary>
        /// Instantiates a FT4222 Device object.
        /// </summary>
        /// <param name="ftdevice">a FT Device</param>
        public Ft4222Device(FtDevice ftdevice)
            : base(ftdevice.Flags, ftdevice.Type, ftdevice.Id, ftdevice.LocId, ftdevice.SerialNumber, ftdevice.Description)
        {
        }

        /// <summary>
        /// Creates an I2C Bus
        /// </summary>
        /// <returns>An I2C bus</returns>
        public I2cBus CreateI2cBus() => new Ft4222I2cBus(this);
    }
}
