# Cpu Temperature

Device bindings for the CPU Temperature Sensor. Returns the current temperature of the CPU Temperature Sensor. Useful telemetry in its own right, but also useful for calibrating the Raspberry Pi Sense HAT.

## Usage

On Windows, this tries to use the OpenHardwareMonitor binding (see there for details).
If it is not available, some guesswork is done to get a temperature sensor. However, the temperature returned by this binding may not be the actual CPU temperature, but one of the mainboard sensors instead. Therefore, depending on the mainboard, no data may be available. Unless OpenHardwareMonitor can be used, elevated permissions ("Admin rights") are required.

```csharp
sing System;
using System.Threading;
using Iot.Device.CpuTemperature;

CpuTemperature cpuTemperature = new CpuTemperature();
Console.WriteLine("Press any key to quit");

while (!Console.KeyAvailable)
{
    if (cpuTemperature.IsAvailable)
    {
        var temperature = cpuTemperature.ReadTemperatures();
        foreach (var entry in temperature)
        {
            if (!double.IsNaN(entry.Temperature.DegreesCelsius))
            {
                Console.WriteLine($"Temperature from {entry.Sensor.ToString()}: {entry.Temperature.DegreesCelsius} °C");
            }
            else
            {
                Console.WriteLine("Unable to read Temperature.");
            }
        }
    }
    else
    {
        Console.WriteLine($"CPU temperature is not available");
    }

    Thread.Sleep(1000);
}

cpuTemperature.Dispose();
```
