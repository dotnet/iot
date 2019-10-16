using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Iot.Device.OneWire
{
    public partial class OneWireBus
    {
        internal const string SysfsBusDevicesPath = "/sys/bus/w1/devices";
        internal const string SysfsDevicesPath = "/sys/devices";

        private static IEnumerable<string> EnumerateBusIdsInternal()
        {
            foreach (var entry in Directory.EnumerateDirectories(SysfsBusDevicesPath, "w1_bus_master*"))
            {
                yield return Path.GetFileName(entry);
            }
        }

        internal IEnumerable<OneWireDevice> EnumerateDevicesInternal(DeviceFamily family)
        {
            var devNames = File.ReadLines(Path.Combine(SysfsDevicesPath, BusId, "w1_master_slaves"));
            foreach (var devName in devNames)
            {
                int.TryParse(devName.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var devFamily);
                switch (family)
                {
                    case DeviceFamily.Any:
                        yield return CreateDeviceByFamily(devName, (DeviceFamily)devFamily);
                        break;
                    case DeviceFamily.DigitalThermometer:
                        if (devFamily == 0x10 || devFamily == 0x28 || devFamily == 0x3B || devFamily == 0x42)
                            yield return CreateDeviceByFamily(devName, (DeviceFamily)devFamily);
                        break;
                    default:
                        if (devFamily == (int)family)
                            yield return CreateDeviceByFamily(devName, (DeviceFamily)devFamily);
                        break;
                }
            }
        }

        internal static async Task ScanForDeviceChangesInternal(OneWireBus bus, int numDevices, int numScans)
        {
            if (numDevices > 0)
                await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_max_slave_count"), numDevices.ToString());

            await File.WriteAllTextAsync(Path.Combine(SysfsDevicesPath, bus.BusId, "w1_master_search"), numScans.ToString());
        }
    }
}
