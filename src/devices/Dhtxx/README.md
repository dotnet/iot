# DHTxx Sensors

## Summary

The DHT temperature and humidity sensors are very popular. Most used sensors are DHT11 and DHT22. Both are supported by this library.

## Device Family

Devide Familly contains DHT11 and DHT22 which are very popular.

**DHT11** [datasheet in chineese](https://cdn-shop.adafruit.com/datasheets/DHT11-chinese.pdf)
**DHT22** [datasheet](https://cdn-shop.adafruit.com/datasheets/DHT22.pdf)

Note: other DHT components are not supported but code can be adjusted to have them working

## Supported platforms

**Both DHT has been tested on a Raspberry Pi 3 Model B Rev 2**. Sensors has not yet been tested on other platforms. If you encounter issues running on other platforms, try to adjust the value ```waitMS``` in the ```DhtSensor.cs``` file.

```csharp
byte waitMS = 99;
#if DEBUG
    waitMS = 27;
#endif
```

This value is used to wait the right amount of time between to reading in a for loop. 

## Usage

Usage is straight forward and you will find more explanations in the [example](./samples/README.md).

```csharp
using System;
using Iot.Device.DHTxx;
using System.Diagnostics;
using System.Device.Gpio;
class Program
{   
    static void Main(string[] args)
    {
        Console.WriteLine("Hello DHT!");
        DHTSensor dht = new DHTSensor(26, DhtType.Dht22);
        while(true)
        {
            bool readret = dht.ReadData();
            if (readret)
                Console.WriteLine($"Temperature: {dht.Temperature} °C, Humidity: {dht.Humidity} %");
            else
                Console.WriteLine("Error reading the sensor");
            System.Threading.Thread.Sleep(1000);
        }
        
    }
}
```

Once the sensor is created, you need to read first, make sure the read is successful and then you can get the Temperature and Humidity.


