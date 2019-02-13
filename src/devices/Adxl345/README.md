# ADXL345
ADXL345 is a small, thin, low power, 3-axis accelerometer with high resolution (13-bit) measurement at up to ±16g.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Adxl345.SpiClockFrequency,
    Mode = Adxl345.SpiMode
};

// get SpiDevice(In Linux)
UnixSpiDevice device = new UnixSpiDevice(settings);

// get SpiDevice(In Win10)
// Windows10SpiDevice device = new Windows10SpiDevice(settings);

// pass in a SpiDevice
// set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(device, GravityRange.Range04))
{
    // read acceleration
    Vector3 data = sensor.Acceleration;

    //use sensor
}
```

## References
In Chinese : http://wenku.baidu.com/view/87a1cf5c312b3169a451a47e.html

In English : https://www.analog.com/media/en/technical-documentation/data-sheets/ADXL345.pdf
