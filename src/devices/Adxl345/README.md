# ADXL345 - Accelerometer

ADXL345 is a small, thin, low power, 3-axis accelerometer with high resolution (13-bit) measurement at up to ±16g.

## Documentation

In [Chinese](http://wenku.baidu.com/view/87a1cf5c312b3169a451a47e.html)

In [English](https://www.analog.com/media/en/technical-documentation/data-sheets/ADXL345.pdf)

## Sensor Image

![sensor](sensor.jpg)

## Usage

```csharp
SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Adxl345.SpiClockFrequency,
    Mode = Adxl345.SpiMode
};

var device = SpiDevice.Create(settings);

// set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(device, GravityRange.Range04))
{
    // read acceleration
    Vector3 data = sensor.Acceleration;

    //use sensor
}
```

## Example

### Hardware Required

* ADXL345
* Male/Female Jumper Wires

## Circuit

![cicuit](ADXL345_circuit_bb.png)

* VCC - 3.3 V
* GND -  GND
* CS - CS0(Pin24)
* SDO - SPI0 MISO(Pin21)
* SDA - SPI0 MOSI (Pin19)
* SCL - SPI0 SCLK(Pin23)

### Code

```csharp
SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Adxl345.SpiClockFrequency,
    Mode = Adxl345.SpiMode
};
var device = SpiDevice.Create(settings);

// Set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(device, GravityRange.Range04))
{
    // loop
    while (true)
    {
        // read data
        Vector3 data = sensor.Acceleration;

        Console.WriteLine($"X: {data.X.ToString("0.00")} g");
        Console.WriteLine($"Y: {data.Y.ToString("0.00")} g");
        Console.WriteLine($"Z: {data.Z.ToString("0.00")} g");
        Console.WriteLine();

        // wait for 500ms
        Thread.Sleep(500);
    }
}
```

### Result

![running result](RunningResult.jpg)
