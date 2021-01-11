## Samples
The Max31856.samples file contains a sample usage of the device. Note that this reads two temperatures. One is a connected thermocouple reading which can be read using the  ```TryGetTemperature``` command and the other is the temperature of the device itself which can be read using the ```GetColdJunctionTemperature``` command. The Cold Junction Temperature is used internally to increase the accuracy of the thermocouple but can also be read if you find a use for it.

Create a new ```SpiConnectionSettings``` Class if using a Raspberry Pi do not change these settings.

```csharp
SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = Max31856.SpiClockFrequency,
    Mode = Max31856.SpiMode,
    DataFlow = 0
};
```

Create a new ```SpiDevice``` with the settings from above. Then create a new Max31856 device with the ```SpiDevice``` as well as the correct ```ThermocoupleType``` (see note below)
```csharp
using SpiDevice device = SpiDevice.Create(settings);
using Max31856 sensor = new(device, ThermocoupleType.K);
```

Now read the temperature from the device. Using the UnitsNet nuget you can see the units of your choosing. In this example you chan change```DegreesFahrenheit``` to ```DegreesCelsius``` or any other unit by changing ```.GetTemperature().DegreesFahrenheit``` to another unit of your choice.

```csharp
while (true)
{
    Temperature tempColdJunction = sensor.GetColdJunctionTemperature();
    Console.WriteLine($"Temperature: {tempColdJunction} ℃");
    Thread.Sleep(2000);
}
```
