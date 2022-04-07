# OpenHardwareMonitor client library

Client binding for OpenHardwareMonitor. Returns a set of sensor measurements for the current hardware. The values include CPU temperature(s), Fan Speed(s), GPU Temperature(s) and Hard Disk states. The set of values is hardware dependent.

## Documentation

This binding works on Windows only. It requires that OpenHardwareMonitor (<https://openhardwaremonitor.org/>) is running in the background. While that tool requires elevated permissions to work, the binding (and the application using it) does not. Check out <https://github.com/hexagon-oss/openhardwaremonitor> for an improved fork with some additional features. The latest version contains a new transport protocol between the tool and the binding (HTTP instead of WMI), which improves the reliability of the data and additionally allows reading the performance data from a remote computer.

The binding supports some additional, "virtual" sensor measuments that are derived from other values. The following extra values are provided:

- For each sensor returning power, another is generated which calculates energy consumed (by integrating the values over time).
- From the CPU Package power sensor, the CPU heat flux is calculated (estimating the size of the die).
- If both a power and a voltage sensor are available for CPU Package, the current is calculated.

## Usage

```csharp
using System;
using System.Globalization;
using System.Threading;
using Iot.Device.HardwareMonitor;
using UnitsNet;

Console.WriteLine("Press any key to quit");

OpenHardwareMonitor hw = new OpenHardwareMonitor();

// Explicit update - exactly one data set is read for each iteration
hw.UpdateStrategy = SensorUpdateStrategy.SynchronousExplicit;

if (hw.GetSensorList().Count == 0)
{
    Console.WriteLine("OpenHardwareMonitor is not running");
    return;
}

hw.EnableDerivedSensors();

while (!Console.KeyAvailable)
{
    Console.Clear();
    Console.WriteLine("Showing all available sensors (press any key to quit)");
    var components = hw.GetHardwareComponents();
    foreach (var component in components)
    {
        Console.WriteLine("--------------------------------------------------------------------");
        Console.WriteLine($"{component.Name} Type {component.Type}, Path {component.Identifier}");
        Console.WriteLine("--------------------------------------------------------------------");
        foreach (var sensor in hw.GetSensorList(component))
        {
            Console.Write($"{sensor.Name}: Path {sensor.Identifier}, Parent {sensor.Parent} ");
            if (sensor.TryGetValue(out IQuantity? quantity))
            {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1:g}", quantity!.Type, quantity));
            }
            else
            {
                Console.WriteLine($"No data");
            }
        }
    }

    if (hw.TryGetAverageGpuTemperature(out Temperature gpuTemp) &&
        hw.TryGetAverageCpuTemperature(out Temperature cpuTemp))
    {
        Console.WriteLine($"Averages: CPU temp {cpuTemp:s2}, GPU temp {gpuTemp:s2}, CPU Load {hw.GetCpuLoad()}");
    }

    Thread.Sleep(1000);
}
```
