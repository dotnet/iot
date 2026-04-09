// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire device.
    /// </summary>
    public class OneWireDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireDevice"/> class.
        /// </summary>
        /// <param name="busId">The 1-wire bus the device is found on.</param>
        /// <param name="devId">The id of the device.</param>
        public OneWireDevice(string busId, string devId)
            : this(busId, devId, OneWireBus.DefaultSysfsDevicesPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireDevice"/> class with a custom sysfs devices path.
        /// This constructor allows overriding the default sysfs path for testing or non-standard environments.
        /// </summary>
        /// <param name="busId">The 1-wire bus the device is found on.</param>
        /// <param name="devId">The id of the device.</param>
        /// <param name="sysfsDevicesPath">The sysfs path for device access (default: "/sys/devices").</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public OneWireDevice(string busId, string devId, string sysfsDevicesPath)
        {
            BusId = busId ?? throw new ArgumentNullException(nameof(busId));
            DeviceId = devId ?? throw new ArgumentNullException(nameof(devId));
            SysfsDevicesPath = sysfsDevicesPath ?? throw new ArgumentNullException(nameof(sysfsDevicesPath));
            Family = OneWireBus.GetDeviceFamilyInternal(busId, devId);
        }

        /// <summary>
        /// Enumerate all devices found on 1-wire busses in this system.
        /// </summary>
        /// <param name="family">Family id used to filter devices.</param>
        /// <returns>A list of devices found.</returns>
        public static IEnumerable<(string BusId, string DevId)> EnumerateDeviceIds(DeviceFamily family = DeviceFamily.Any)
        {
            foreach (var busId in OneWireBus.EnumerateBusIdsInternal(OneWireBus.DefaultSysfsBusDevicesPath))
            {
                foreach (var devId in OneWireBus.EnumerateDeviceIdsInternal(OneWireBus.DefaultSysfsDevicesPath, busId, family))
                {
                    yield return (busId, devId);
                }
            }
        }

        /// <summary>
        /// The bus where this device is attached.
        /// </summary>
        public string BusId { get; }

        /// <summary>
        /// The 1-wire id of this device.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// The device family id of this device.
        /// </summary>
        public DeviceFamily Family { get; }

        /// <summary>
        /// The sysfs path used for device access.
        /// </summary>
        internal string SysfsDevicesPath { get; }
    }
}
