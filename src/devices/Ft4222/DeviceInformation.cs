// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 device information
    /// </summary>
    public class DeviceInformation
    {
        /// <summary>
        /// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
        /// </summary>
        public FtFlag Flags { get; set; }

        /// <summary>
        /// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        public FtDevice Type { get; set; }

        /// <summary>
        /// The Vendor ID and Product ID of the device
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The physical location identifier of the device
        /// </summary>
        public uint LocId { get; set; }

        /// <summary>
        /// The device serial number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// The device description
        /// </summary>
        public string Description { get; set; }
    }
}
