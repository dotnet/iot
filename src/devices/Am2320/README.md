# AM2320 - Temperature and Humidity sensor

AM2320 is a temperature and humidity sensor, sensible to 0.1 degree and 0.1 relative humidity.

## Documentation

- [AM2320](https://cdn-shop.adafruit.com/product-files/3721/AM2320.pdf)

## Usage

Here is an example how to use the AM2320:

```csharp
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Am2320;

Debug.WriteLine("Hello from AM2320!");

// Important: make sure the bus speed is in standar mode and not in fast more.
using Am2320 am2330 = new(I2cDevice.Create(new I2cConnectionSettings(1, Am2320.DefaultI2cAddress)));

while (true)
{
    if (am2330.TryReadTemperature(out Temperature temp))
    {
        Debug.Write($"Temp = {temp.DegreesCelsius} C. ");
    }
    else
    {
        Debug.WriteLine("Can't read temperature. ");
    }

    if (am2330.TryReadHumidity(out RelativeHumidity hum))
    {
        Debug.WriteLine($"Hum = {hum.Percent} %.");
    }
    else
    {
        Debug.WriteLine("Can't read humidity.");
    }

    Thread.Sleep(Am2320.MinimumReadPeriod);
}
```

### Device Information

You can read the Device Information.

> Note: on some copies, the device information only returns 0.

```csharp
// On some copies, the device information contains only 0
var deviceInfo = am2330.DeviceInformation;
if (deviceInfo != null)
{
    Debug.WriteLine($"Model: {deviceInfo.Model}");
    Debug.WriteLine($"Version: {deviceInfo.Version}");
    Debug.WriteLine($"Device ID: {deviceInfo.DeviceId}");
}
```

## Limitations

Only the I2C implementation is available, not the 1 wire one.

The user registers and the status register are not implemented. The status register is just a register the user can store data. It is not currently used for any usage according to the documentation.
