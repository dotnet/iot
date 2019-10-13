// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// Initializes a new instance of the <see cref="OneWireDevice"/> class
        /// </summary>
        /// <param name="bus">The 1-wire bus the device is found on</param>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="family">The 1-wire fmily id</param>
        protected internal OneWireDevice(OneWireBus bus, string deviceId, DeviceFamily family)
        {
            if (family <= 0 || (int)family > 0xff)
                throw new ArgumentException(nameof(family));
            Bus = bus;
            DeviceId = deviceId;
            Family = family;
        }

        /// <summary>
        /// Enumerate all devices found on 1-wire busses in this system.
        /// </summary>
        /// <param name="family">Family id used to filter devices.</param>
        /// <returns>A list of devices found.</returns>
        public static IEnumerable<OneWireDevice> EnumerateDevices(DeviceFamily family = DeviceFamily.Any)
        {
            foreach (var bus in OneWireBus.EnumerateBuses())
            {
                foreach (var dev in bus.EnumerateDevices(family))
                {
                    yield return dev;
                }
            }
        }

        /// <summary>
        /// The bus where this device is attached.
        /// </summary>
        public OneWireBus Bus { get; }

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
