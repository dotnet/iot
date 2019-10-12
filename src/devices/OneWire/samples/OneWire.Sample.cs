// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;

namespace Iot.Device.OneWire.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Make sure you can access the bus device before requesting a device scan
            // $ sudo chmod a+rw /sys/bus/w1/devices/w1_bus_master1/w1_master_*
            foreach (var bus in OneWireController.EnumerateBuses())
            {
                Console.WriteLine($"Found bus '{bus.DeviceId}', scanning for devices ...");
                await bus.ScanForDevicesAsync();
                foreach (var dev in bus.EnumerateDevices())
                {
                    Console.WriteLine($"Found family '{(int)dev.Family:x2}' device '{dev.DeviceId}' on master '{bus.DeviceId}'");
                    if (dev is OneWireThermometerDevice devTemp)
                    {
                        Console.WriteLine("Temperature reported by device: " + (await devTemp.ReadTemperatureAsync()).Celsius.ToString("F2") + "°C");
                    }
                }
            }
        }
    }
}
