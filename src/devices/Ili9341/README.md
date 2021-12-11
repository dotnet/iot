# Ili9341 TFT LCD Controller

The ILI9341 is a QVGA (Quarter VGA) driver integrated circuit that is used to control 240×320 VGA LCD screens

## Documentation

[Adafruit ILI9341 Arduino Library](https://github.com/adafruit/Adafruit_ILI9341)

## Device Family

- ILI9341 [datasheet](https://cdn-shop.adafruit.com/datasheets/ILI9341.pdf)

### Related Devices

- [2.4" TFT LCD with Touchscreen Breakout w/MicroSD Socket - ILI9341](https://www.adafruit.com/product/2478#technical-details)
- [2.2" TFT Display](https://learn.adafruit.com/2-2-tft-display)

## Usage

```csharp
using System.Device.Spi;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Ili9341;

const int pinID_DC = 25;
const int pinID_Reset = 24;

using Bitmap dotnetBM = new(240, 320);
using Graphics g = Graphics.FromImage(dotnetBM);
using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ili9341 ili9341 = new(displaySPI, pinID_DC, pinID_Reset);

while (true)
{
    foreach (string filepath in Directory.GetFiles(@"images", "*.png").OrderBy(f => f))
    {
        using Bitmap bm = (Bitmap)Bitmap.FromFile(filepath);
        g.Clear(Color.Black);
        g.DrawImageUnscaled(bm, 0, 0);
        ili9341.SendBitmap(dotnetBM);
        Task.Delay(1000).Wait();
    }
}
```

## Binding Notes

This binding currently only supports commands and raw data. Eventually, the plan is to create a graphics library that can send text and images to the device.

The following connection types are supported by this binding.

- [X] SPI
