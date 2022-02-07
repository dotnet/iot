# WS2812B - Intelligent control LED integrated light source

WS2812B is a intelligent control LED light source that the control circuit and RGB chip are integrated in a package of 5050 components.

It internal include intelligent digital port data latch and signal reshaping amplification drive circuit.

Also include a precision internal oscillator and a voltage programmable constant current control part, effectively ensuring the pixel point light color height consistent.

## Documentation

- [Introduce](http://www.world-semi.com/Certifications/WS2812B.html)
- [Datasheet](http://www.world-semi.com/DownLoadFile/108)

## Typical application circuit

![circuit](circuit.png)

## Usage

Here is an example how to use the APA102:

```csharp
using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Apa102;

var random = new Random();

using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 20_000_000,
    DataFlow = DataFlow.MsbFirst,
    Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
});
using Apa102 apa102 = new Apa102(spiDevice, 16);

while (true)
{
    for (var i = 0; i < apa102.Pixels.Length; i++)
    {
        apa102.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
    }

    apa102.Flush();
    Thread.Sleep(1000);
}
```
