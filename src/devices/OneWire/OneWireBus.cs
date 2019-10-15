// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire bus.
    /// </summary>
    public partial class OneWireBus
    {
        internal OneWireBus(string busId)
        {
            BusId = busId;
        }

        /// <summary>
        /// The 1-wire device id.
        /// </summary>
        public string BusId { get; }

        /// <summary>
        /// Enumerate all 1-wire busses in the system.
        /// </summary>
        /// <returns>A list of discovered busses.</returns>
        public static IEnumerable<OneWireBus> EnumerateBuses()
        {
            return EnumerateBusesInternal();
        }

        internal OneWireDevice CreateDeviceByFamily(string deviceId, DeviceFamily family)
        {
            switch (family)
            {
                case DeviceFamily.Ds18S20:
                case DeviceFamily.Ds18B20: // or Max31820
                case DeviceFamily.Ds1825: // or Max31826, Max31850
                case DeviceFamily.Ds28EA00:
                    return new OneWireThermometerDevice(this, deviceId, family);
                default:
                    return new OneWireDevice(this, deviceId, family);
            }
        }

        /// <summary>
        /// Enumerates all devices on this bus.
        /// </summary>
        /// <param name="family">Family id used to filter enumerated devices.</param>
        /// <returns>A list of discovered devices.</returns>
        public IEnumerable<OneWireDevice> EnumerateDevices(DeviceFamily family = DeviceFamily.Any)
        {
            return EnumerateDevicesInternal(family);
        }

        /// <summary>
        /// Start a new scan for devices on the bus.
        /// </summary>
        /// <param name="numDevices">Max number of devices to scan for before finishing.</param>
        /// <param name="numScans">Number of scans to do to find numDevices devices.</param>
        /// <returns>Task representing the async work.</returns>
        public Task ScanForDevicesAsync(int numDevices = 64, int numScans = -1)
        {
            // Default 64 used to align with Linux driver
            // https://github.com/torvalds/linux/blob/v5.3/drivers/w1/w1.c#L46

            return ScanForDevicesInternal(this, numDevices, numScans);
        }
    }
}
