// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        {
            BusId = busId;
            DeviceId = devId;
            Family = OneWireBus.GetDeviceFamilyInternal(busId, devId);
        }

        /// <summary>
        /// Enumerate all devices found on 1-wire busses in this system.
        /// </summary>
        /// <param name="family">Family id used to filter devices.</param>
        /// <returns>A list of devices found.</returns>
        public static IEnumerable<(string busId, string devId)> EnumerateDeviceIds(DeviceFamily family = DeviceFamily.Any)
        {
            foreach (var busId in OneWireBus.EnumerateBusIdsInternal())
            {
                foreach (var devId in OneWireBus.EnumerateDeviceIdsInternal(busId, family))
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
    }
}
