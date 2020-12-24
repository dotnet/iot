# MAX31865

## Summary
The MAX31865 device is a SPI interface Resistance Temperature Detector to Digital Converter.

## Sensor Image
![Illustration of wiring from a Raspberry Pi device](device.jpg)

## Usage
The MAX31856.samples file contains a sample usage of the device. Note that this reads two temperatures. One is a connected thermocouple reading which can be read using the  ```TryGetTemperature``` command and the other is the temperature of the device itself which can be read using the ```GetColdJunctionTemperature``` command. The Cold Junction Temperature is used internally to increase the accuracy of the thermocouple but can also be read if you find a use for it.

Create a new ```SpiConnectionSettings``` Class if using a Raspberry Pi do not change these settings.

```csharp
SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = MAX31856.SpiClockFrequency,
    Mode = MAX31856.SpiMode,
    DataFlow = 0
};
```

Create a new ```SpiDevice``` with the settings from above. Then create a new MAX31856 device with the ```SpiDevice``` as well as the correct ```ThermocoupleType``` (see note below)
```csharp
using SpiDevice device = SpiDevice.Create(settings);
using MAX31856 sensor = new(device, ThermocoupleType.K);
```

Now read the temperature from the device. Using the UnitsNet nuget you can see the units of your choosing. In this example you chan change```DegreesFahrenheit``` to ```DegreesCelsius``` or any other unit by changing ```.GetTemperature().DegreesFahrenheit``` to another unit of your choice.

```csharp
while (true)
{
    // Reads temperature if the device is not reading properly
    var tempColdJunction = sensor.GetColdJunctionTemperature();
    if (sensor.TryGetTemperature(out Temperature temperature))
    {
        Console.WriteLine($"Temperature: {temperature.DegreesFahrenheit:0.0000000} °F, Cold Junction: {tempColdJunction.DegreesFahrenheit:0.00} °F");
    }
    else
    {
        Console.WriteLine($"Error reading temperature, Cold Junction temperature: {tempColdJunction.DegreesFahrenheit:0.00}");
    }

    // wait for 2000ms
    Thread.Sleep(2000);
```

**Note:** _ThermocoupleType.K is configured for a K type thermocouple if you want to use a B,E,J,K,N,R,S, or T simply change the K to the thermocouple type of your choosing._

## References 

**MAX31865** [datasheet](https://datasheets.maximintegrated.com/en/ds/MAX31865.pdf)
