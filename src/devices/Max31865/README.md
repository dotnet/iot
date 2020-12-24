# MAX31865

## Summary
The MAX31865 device is a SPI interface Resistance Temperature Detector to Digital Converter.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = Max31865.SpiClockFrequency,
    Mode = Max31865.SpiMode1,
    DataFlow = Max31865.SpiDataFlow
};

using SpiDevice device = SpiDevice.Create(settings);
using Max31865 sensor = new(device, PlatinumResistanceThermometerType.PT1000, ResistanceTemperatureDetectorWires.ThreeWire, 4300);

while (true)
{
    Console.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");

    // wait for 2000ms
    Thread.Sleep(2000);
}
```

**Note:** You can use any PT100 or PT1000 temperature sensor which has 2/3/4 wires._

## References 

**MAX31865** [datasheet](https://datasheets.maximintegrated.com/en/ds/MAX31865.pdf)
