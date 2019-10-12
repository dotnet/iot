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
    public class OneWireBus
    {
        internal OneWireBus(string deviceId)
        {
            DeviceId = deviceId;
        }

        /// <summary>
        /// The 1-wire device id.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// Different device families supported by the driver.
        /// </summary>
        public enum DeviceFamily : int
        {
            /// <summary>
            /// Special device family used when enumerating all devices on a bus.
            /// </summary>
            Any = -1,
            /// <summary>
            /// Family id of a DS18B20 temperature sensor.
            /// </summary>
            Ds18B20 = 0x28,
            /// <summary>
            /// Family id of a MAX31820 temperature sensor.
            /// </summary>
            Max31820 = 0x28,
            /// <summary>
            /// Special family id used to enumerate all temperature sensors.
            /// </summary>
            DigitalThermometer = 0x100,
        }

        /// <summary>
        /// Enumerates all devices on this bus.
        /// </summary>
        /// <param name="family">Family id used to filter enumerated devices.</param>
        /// <returns>A list of discovered devices.</returns>
        public IEnumerable<OneWireDevice> EnumerateDevices(DeviceFamily family = DeviceFamily.Any)
        {
            return OneWireController.EnumerateDevices(this, family);
        }

        /// <summary>
        /// Start a new scan for devices on the bus.
        /// </summary>
        /// <param name="numDevices">Max number of devices to scan for before finishing.</param>
        /// <param name="numScans">Number of scans to do to find numDevices devices.</param>
        /// <returns>Task representing the async work.</returns>
        public Task ScanForDevicesAsync(int numDevices = 5, int numScans = -1)
        {
            return OneWireController.ScanForDevicesInternal(this, numDevices, numScans);
        }
    }
}
