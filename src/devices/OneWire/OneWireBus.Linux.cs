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
        internal static string SysfsBusDevicesPath = "/sys/bus/w1/devices";
        internal static string SysfsDevicesPath = "/sys/devices";

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
            int.TryParse(devId.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var devFamily);
            return (DeviceFamily)devFamily;
        }

        internal static async Task ScanForDeviceChangesInternalAsync(OneWireBus bus, int numDevices, int numScans)
        {
#if NETSTANDARD2_0
            await Task.Factory.StartNew(() =>
            {
                if (numDevices > 0)
                {
                    File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());
                }

                File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
            });
#else
            if (numDevices > 0)
            {
                await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
#endif
        }

        internal static void ScanForDeviceChangesInternal(OneWireBus bus, int numDevices, int numScans)
        {
            if (numDevices > 0)
            {
                File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());
            }

            File.WriteAllText(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
        }

        /// <summary>
        /// Sets the bus path to the specified directory for testing purpose.
        /// </summary>
        /// <param name="path">The path to the directory that will be set as the bus path. This path must exist and be a valid directory.</param>
        /// <exception cref="ArgumentException">Thrown if the specified path does not exist or is not a directory.</exception>
        public static void OverwriteSysfsBusDevicesPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Provided path {path} does not exist or is not a directory.");
            }

            SysfsBusDevicesPath = path;
        }

        /// <summary>
        /// Sets the devices path to the specified directory for testing purpose.
        /// </summary>
        /// <param name="path">The path to the directory that will be set as the devices path. This path must exist and be a valid directory.</param>
        /// <exception cref="ArgumentException">Thrown if the provided path does not exist or is not a directory.</exception>
        public static void OverwriteSysfsDevicesPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Provided path {path} does not exist or is not a directory.");
            }

            SysfsDevicesPath = path;
        }
    }
}
