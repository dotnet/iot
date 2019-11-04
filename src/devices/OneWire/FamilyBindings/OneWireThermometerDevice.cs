// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire thermometer device.
    /// </summary>
    public partial class OneWireThermometerDevice : OneWireDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireThermometerDevice"/> class.
        /// </summary>
        /// <param name="busId">The 1-wire bus the device is found on.</param>
        /// <param name="devId">The id of the device.</param>
        public OneWireThermometerDevice(string busId, string devId)
            : base(busId, devId)
        {
        }

        /// <summary>
        /// Check if family is compatible with this type of devices.
        /// </summary>
        /// <param name="family">The family to check.</param>
        /// <returns>Returns true if device is compatible.</returns>
        public static bool IsCompatible(DeviceFamily family)
        {
            return family == DeviceFamily.Ds18s20 ||
                   family == DeviceFamily.Ds18b20 ||
                   family == DeviceFamily.Ds1825 ||
                   family == DeviceFamily.Ds28ea00;
        }

        /// <summary>
        /// Check if device is compatible with this family of devices.
        /// </summary>
        /// <param name="busId">The 1-wire bus the device is found on.</param>
        /// <param name="devId">The id of the device.</param>
        /// <returns>Returns true if device is compatible.</returns>
        public static bool IsCompatible(string busId, string devId)
        {
            var family = OneWireBus.GetDeviceFamilyInternal(busId, devId);
            return IsCompatible(family);
        }

        /// <summary>
        /// Enumerate all devices in system of type thermometer.
        /// </summary>
        /// <returns>A list of thermometer devices.</returns>
        public static IEnumerable<OneWireThermometerDevice> EnumerateDevices()
        {
            foreach (var (busId, devId) in EnumerateDeviceIds(DeviceFamily.Thermometer))
            {
                yield return new OneWireThermometerDevice(busId, devId);
            }
        }

        /// <summary>
        /// Reads the current temperature of the device.
        /// Expect this function to be slow (about one second).
        /// </summary>
        /// <returns>The read temperature value.</returns>
        public Task<Temperature> ReadTemperatureAsync()
        {
            return ReadTemperatureInternalAsync();
        }

        /// <summary>
        /// Reads the current temperature of the device.
        /// Expect this function to be slow (about one second).
        /// </summary>
        /// <returns>The read temperature value.</returns>
        public Temperature ReadTemperature()
        {
            return ReadTemperatureInternal();
        }
    }
}
