// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public partial class OneWireController
    {
        internal const string SysfsBusDevicesPath = "/sys/bus/w1/devices";
        internal const string SysfsDevicesPath = "/sys/devices";

        private static IEnumerable<OneWireBus> EnumerateBusesInternal()
        {
            foreach (var entry in Directory.EnumerateDirectories(SysfsBusDevicesPath, "w1_bus_master*"))
            {
                yield return new OneWireBus(Path.GetFileName(entry));
            }
        }

        internal static IEnumerable<OneWireDevice> EnumerateDevices(OneWireBus bus, int family)
        {
            var devNames = File.ReadLines(Path.Combine(SysfsDevicesPath, bus.DeviceId, "w1_master_slaves"));
            foreach (var devName in devNames)
            {
                int.TryParse(devName.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var devFamily);
                if (family > 0 && devFamily == family || family == -1 && devFamily > 0 && devFamily <= 255)
                    yield return CreateDeviceByFamily(bus, devName, devFamily);
            }
        }

        internal static async Task ScanForDevicesInternal(OneWireBus bus, int numDevices, int numScans)
        {
            await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.DeviceId, "w1_master_max_slave_count"), numDevices.ToString());
            await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.DeviceId, "w1_master_search"), numScans.ToString());
        }
    }
}
