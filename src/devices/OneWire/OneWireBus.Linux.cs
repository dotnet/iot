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
        internal const string DefaultSysfsBusDevicesPath = "/sys/bus/w1/devices";
        internal const string DefaultSysfsDevicesPath = "/sys/devices";

        private readonly string _sysfsBusDevicesPath = DefaultSysfsBusDevicesPath;
        private readonly string _sysfsDevicesPath = DefaultSysfsDevicesPath;

        internal static IEnumerable<string> EnumerateBusIdsInternal(string sysfsBusDevicesPath)
        {
            foreach (var entry in Directory.EnumerateDirectories(sysfsBusDevicesPath, "w1_bus_master*"))
            {
                yield return Path.GetFileName(entry);
            }
        }

        internal static IEnumerable<string> EnumerateDeviceIdsInternal(string sysfsDevicesPath, string busId, DeviceFamily family)
        {
            var devIds = File.ReadLines(Path.Combine(sysfsDevicesPath, busId, "w1_master_slaves"));
            return family switch
            {
                DeviceFamily.Any => devIds,
                DeviceFamily.Thermometer => devIds.Where(devId => OneWireThermometerDevice.IsCompatible(busId, devId)),
                _ => devIds.Where(devId => GetDeviceFamilyInternal(busId, devId) == family),
            };
        }

        internal static DeviceFamily GetDeviceFamilyInternal(string busId, string devId)
        {
            int.TryParse(devId.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var devFamily);
            return (DeviceFamily)devFamily;
        }

        private async Task ScanForDeviceChangesInternalAsync(int numDevices, int numScans)
        {
#if NETSTANDARD2_0
            var sysfsDevicesPath = _sysfsDevicesPath;
            var busId = BusId;
            await Task.Factory.StartNew(() =>
            {
                if (numDevices > 0)
                {
                    File.WriteAllText(Path.Combine(sysfsDevicesPath, busId, "w1_master_max_slave_count"), numDevices.ToString());
                }

                File.WriteAllText(Path.Combine(sysfsDevicesPath, busId, "w1_master_search"), numScans.ToString());
            });
#else
            if (numDevices > 0)
            {
                await File.WriteAllTextAsync(Path.Combine(_sysfsDevicesPath, BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            await File.WriteAllTextAsync(Path.Combine(_sysfsDevicesPath, BusId, "w1_master_search"), numScans.ToString());
#endif
        }

        private void ScanForDeviceChangesInternal(int numDevices, int numScans)
        {
            if (numDevices > 0)
            {
                File.WriteAllText(Path.Combine(_sysfsDevicesPath, BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            File.WriteAllText(Path.Combine(_sysfsDevicesPath, BusId, "w1_master_search"), numScans.ToString());
        }
    }
}
