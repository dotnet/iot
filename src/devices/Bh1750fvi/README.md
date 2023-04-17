# BH1750FVI - Ambient Light Sensor

BH1750FVI is an digital Ambient Light Sensor IC for I2C bus interface. This IC is the most suitable to obtain the ambient light data for adjusting LCD and Keypad backlight power of Mobile phone. It is possible to detect wide range at High resolution.

## Documentation

Product datasheet can be found [here](https://www.mouser.com/datasheet/2/348/bh1750fvi-e-186247.pdf)

## Sensor Image

![sensor](sensor.jpg)

## Usage

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(busId: 1, (int)I2cAddress.AddPinLow);
I2cDevice device = I2cDevice.Create(settings);

using (Bh1750fvi sensor = new Bh1750fvi(device))
{
    // read illuminance(Lux)
    double illuminance = sensor.Illuminance;
}

```

## Circuit

![circuit](BH1750FVI_Circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
* ADDR - GND

Result of the sample:

![running result](RunningResult.jpg)
