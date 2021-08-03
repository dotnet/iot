# Ws28xx LED drivers

This binding allows you to update the RGB LEDs on Ws28xx and based strips and matrices.

To see how to use the binding in code, see the [sample](samples/Program.cs).

## Documentation

* WS2812B: [Datasheet](https://cdn-shop.adafruit.com/datasheets/WS2812B.pdf)
* WS2808: [Datasheet](https://datasheetspdf.com/pdf-file/806051/Worldsemi/WS2801/1)
* [Neo pixels guide](https://learn.adafruit.com/adafruit-neopixel-uberguide)
* [Neo pixels x8 stick](https://www.adafruit.com/product/1426)

## Board

### Neo pixels

![Raspberry Pi Breadboard diagram](rpi-neo-pixels_bb.png)

### WS2808

![WS2808 diagram](WS2808.png)

## Usage

```csharp
using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;

// Configure the count of pixels
const int Count = 8;
Console.Clear();

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = 2_400_000,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};
using SpiDevice spi = SpiDevice.Create(settings);

Ws28xx neo = new Ws2808(spi, count);
//Ws28xx neo = new Ws2812b(spi, Count);

while (true)
{
    Rainbow(neo, Count);
    System.Threading.Thread.Sleep(100);
}

void Rainbow(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel((i + j) & 255));
        }

        neo.Update();
    }
}
```

## Binding Notes

### Raspberry Pi setup (/boot/config.txt)

* Make sure spi is enabled

```text
dtparam=spi=on
```

* Make sure SPI don't change speed fix the core clock:

```text
core_freq=250
core_freq_min=250
```
