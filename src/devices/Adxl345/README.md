# ADXL345
ADXL345 is a small, thin, low power, 3-axis accelerometer with high resolution (13-bit) measurement at up to ±16g.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
// SPI bus 0
// CS Pin connect to CS0(Pin24)
// set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(0, 0, GravityRange.Range2))
{
    // read acceleration
    Vector3 data = sensor.Acceleration;

    //TODO
}
```

or

```C#
// Pass in a SpiDevice, like UnixSpiDevice or Windows10SpiDevice
// set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(SpiDevice, GravityRange.Range2))
{
    // read acceleration
    Vector3 data = sensor.Acceleration;

    //TODO
}
```

## References
In Chinese : http://wenku.baidu.com/view/87a1cf5c312b3169a451a47e.html

In English : https://www.analog.com/media/en/technical-documentation/data-sheets/ADXL345.pdf
