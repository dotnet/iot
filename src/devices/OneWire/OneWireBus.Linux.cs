// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public partial class OneWireBus
    {
        internal const string SysfsBusDevicesPath = "/sys/bus/w1/devices";
        internal const string SysfsDevicesPath = "/sys/devices";

        internal static IEnumerable<string> EnumerateBusIdsInternal()
        {
            foreach (var entry in Directory.EnumerateDirectories(SysfsBusDevicesPath, "w1_bus_master*"))
            {
                yield return Path.GetFileName(entry);
            }
        }

        internal static IEnumerable<string> EnumerateDeviceIdsInternal(string busId, DeviceFamily family)
        {
            var devIds = File.ReadLines(Path.Combine(SysfsDevicesPath, busId, "w1_master_slaves"));
            return family switch
            {
                DeviceFamily.Any => devIds,
                DeviceFamily.Thermometer => devIds.Where(devId => OneWireThermometerDevice.IsCompatible(busId, devId)),
                _ => devIds.Where(devId => GetDeviceFamilyInternal(busId, devId) == family),
            };
        }

        internal static DeviceFamily GetDeviceFamilyInternal(string busId, string devId)
        {
            int.TryParse(devId.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var devFamily);
            return (DeviceFamily)devFamily;
        }

        internal static async Task ScanForDeviceChangesInternalAsync(OneWireBus bus, int numDevices, int numScans)
        {
            if (numDevices > 0)
            {
                await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
        }

        internal static void ScanForDeviceChangesInternal(OneWireBus bus, int numDevices, int numScans)
        {
            if (numDevices > 0)
            {
                File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
        }
    }
}
