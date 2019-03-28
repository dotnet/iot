# DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module

## Summary

The DHT temperature and humidity sensors are very popular. Most used sensors are DHT11 and DHT22. Both are supported by this library.

## Device Family

Device Family contains DHT11 and DHT22 which are very popular.

* **DHT11** [datasheet (in chinese)](https://cdn-shop.adafruit.com/datasheets/DHT11-chinese.pdf)
* **DHT22** [datasheet](https://cdn-shop.adafruit.com/datasheets/DHT22.pdf)

Note: other DHT components are not supported but code can be adjusted to have them working

## Supported platforms

**Both DHT has been tested on a Raspberry Pi 3 Model B Rev 2**. Sensors has not yet been tested on other platforms. If you encounter issues running on other platforms, try to adjust the value ```waitMS``` in the ```DhtSensor.cs``` file.

```csharp
byte waitMS = 99;
#if DEBUG
    waitMS = 27;
#endif
```

This value is used to wait 1 microsecond in a for simple loop. This value is platform dependent.

```csharp
for (byte wt = 0; wt < waitMS; wt++)
    ;
```

## Usage

Usage is straight forward and you will find more explanations in the [example](./samples/README.md).

You first need to create a sensor. First parameter is the GPIO pin you want to use and second the DHT type.

```csharp
DHTSensor dht = new DHTSensor(26, DhtType.Dht22);
```

You have 2 ways to read the temperature and humidity. Humidity is a value between 0.0 and 100.0. 100.0 represents 100% humidity in the air.

First one, once the sensor is created, you need to read first, make sure the read is successful and then you can get the Temperature and Humidity.

```csharp
bool readret = dht.ReadData();
if (readret)
    Console.WriteLine($"Temperature: {dht.Temperature.Celsius.ToString("0.00")} °C, Humidity: {dht.Humidity.ToString("0.00")} %");
else
    Console.WriteLine("Error reading the sensor");
```
Second way, is to use the ```TryGetTemperatureAndHumidity``` and the other ```TryGet``` functions. They will return true if the read has been successful and then as an output the temperature, either in Celsius or Fahrenheit and/or relative air humidity.

```csharp
Temperature Temp;
double Hum;
if (dht.TryGetTemperatureAndHumidity(out Temp, out Hum))
    Console.WriteLine($"Temperature: {Temp.Celsius.ToString("0.00")} °C, Humidity: {Hum.ToString("0.00")} %");
else
    Console.WriteLine("Error reading the sensor");
```

Note that functions to read the temperature exist both in Celsius and Fahrenheit.
