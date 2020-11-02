// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire bus.
    /// </summary>
    public partial class OneWireBus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireBus"/> class
        /// </summary>
        /// <param name="busId">The platform id of the 1-wire bus.</param>
        public OneWireBus(string busId)
        {
            BusId = busId;
        }

        /// <summary>
        /// The 1-wire device id.
        /// </summary>
        public string BusId { get; }

        /// <summary>
        /// Enumerate names of all 1-wire busses in the system.
        /// </summary>
        /// <returns>A list of discovered bus ids.</returns>
        public static IEnumerable<string> EnumerateBusIds()
        {
            return EnumerateBusIdsInternal();
        }

        /// <summary>
        /// Enumerates all devices currently detected on this bus. Platform can update device list
        /// periodically. To manually trigger an update call <see cref="ScanForDeviceChangesAsync" />.
        /// </summary>
        /// <param name="family">Family id used to filter enumerated devices.</param>
        /// <returns>A list of discovered devices.</returns>
        public IEnumerable<string> EnumerateDeviceIds(DeviceFamily family = DeviceFamily.Any)
        {
            foreach (var devId in EnumerateDeviceIdsInternal(BusId, family))
            {
                yield return devId;
            }
        }

        /// <summary>
        /// Start a new scan for device changes on the bus.
        /// </summary>
        /// <param name="numDevices">Max number of devices to scan for before finishing. Use -1 for platform default.</param>
        /// <param name="numScans">Number of scans to do to find numDevices devices. Use -1 for platform default.</param>
        /// <returns>Task representing the async work.</returns>
        public Task ScanForDeviceChangesAsync(int numDevices = -1, int numScans = -1)
        {
            return ScanForDeviceChangesInternalAsync(this, numDevices, numScans);
        }

        /// <summary>
        /// Start a new scan for device changes on the bus.
        /// </summary>
        /// <param name="numDevices">Max number of devices to scan for before finishing. Use -1 for platform default.</param>
        /// <param name="numScans">Number of scans to do to find numDevices devices. Use -1 for platform default.</param>
        public void ScanForDeviceChanges(int numDevices = -1, int numScans = -1)
        {
            ScanForDeviceChangesInternal(this, numDevices, numScans);
        }
    }
}
