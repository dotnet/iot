// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire controller.
    /// </summary>
    public static partial class OneWireController
    {
        /// <summary>
        /// Enumerate all 1-wire busses in the system.
        /// </summary>
        /// <returns>A list of discovered busses.</returns>
        public static IEnumerable<OneWireBus> EnumerateBuses()
        {
            return EnumerateBusesInternal();
        }

        /// <summary>
        /// Enumerate all devices found on 1-wire busses in this system.
        /// </summary>
        /// <param name="family">Family id used to filter devices.</param>
        /// <returns>A list of devices found.</returns>
        public static IEnumerable<OneWireDevice> EnumerateDevices(OneWireBus.DeviceFamily family = OneWireBus.DeviceFamily.Any)
        {
            foreach (var bus in EnumerateBuses())
            {
                foreach (var dev in bus.EnumerateDevices(family))
                {
                    yield return dev;
                }
            }
        }

        internal static OneWireDevice CreateDeviceByFamily(OneWireBus bus, string deviceId, OneWireBus.DeviceFamily family)
        {
            switch ((int)family)
            {
                case 0x10:
                    goto case 0x42;
                case 0x28:
                    goto case 0x42;
                case 0x3B:
                    goto case 0x42;
                case 0x42:
                    return new OneWireThermometerDevice(bus, deviceId, family);
                default:
                    return new OneWireDevice(bus, deviceId, family);
            }
        }
    }
}
