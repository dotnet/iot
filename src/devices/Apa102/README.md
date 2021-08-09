# APA102 - Double line transmission integrated control LED

APA102 is a intelligent control LED light source that the control circuit and RGB chip are integrated in a package of 5050/2020 components. It internal include 3 groups shift register and Selfdetection sign decoder circuit.

It's different from WS2812. In addition to the data line, it has a clock line. So APA102 has no strict requirements for timing. It's more friendly to devices such as Raspberry Pi that can't precisely control the timing of data lines.

There are other models like APA107, HD107s, SK9822, etc. The controls are exactly the same.

Model  | SCLK   | PWM
-------|--------|--------
APA102 | 20 MHz | 20 kHz
APA107 | 30 MHz | 9 kHz  
HD107s | 40 MHz | 27 kHz
SK9822 | 15 MHz | 4.7 kHz

## Documentation

- [APA102](https://cdn.instructables.com/ORIG/FC0/UYH5/IOA9KN8K/FC0UYH5IOA9KN8K.pdf)
- [SK9822](https://cdn.instructables.com/ORIG/F66/Q8GE/IOA9KN8U/F66Q8GEIOA9KN8U.pdf)
- [Compare](https://www.instructables.com/id/Compare-SK6822-WS2813-APA102-SK9822/) SK6822 WS2813 APA102 SK9822 LED
- [Difference](https://www.rose-lighting.com/the-difference-of-hd107s-apa107-sk9822led/) between APA102, APA107,HD107s and SK9822

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
