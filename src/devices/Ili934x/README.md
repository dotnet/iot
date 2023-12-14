# Ili934x TFT LCD Controller

This binding supports the ILI9341 and ILI9342 QVGA (Quarter VGA) driver integrated circuits that is used to control 240×320 VGA LCD screens.
The main difference between the two chips is that the ILI9341 is typically used with a screen in portrait mode, while the ILI9342 is used with
screens in landscape mode.

## Documentation

[Adafruit ILI9341 Arduino Library](https://github.com/adafruit/Adafruit_ILI9341)

## Device Family

- ILI9341 [datasheet](https://cdn-shop.adafruit.com/datasheets/ILI9341.pdf)
- ILI9342 [datasheet](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/ILI9342C-ILITEK.pdf)

### Related Devices

- [2.4" TFT LCD with Touchscreen Breakout w/MicroSD Socket - ILI9341](https://www.adafruit.com/product/2478#technical-details)
- [2.2" TFT Display](https://learn.adafruit.com/2-2-tft-display)
- [M5 Though](https://docs.m5stack.com/en/core/tough) + various other devices from M5Stack

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
const int interruptPin = 39;

// This is required to initialize the graphics library
SkiaSharpAdapter.Register();

Chsc6440 touch = null;
using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ili9341 ili9341 = new(displaySPI, pinID_DC, pinID_Reset);

using var image = BitmapImage.CreateFromFile(@"images/Landscape.png");
using var backBuffer = _screen.CreateBackBuffer();

// if a touch controller is attached:
{
    touch = new Chsc6440(I2cDevice.Create(new I2cConnectionSettings(0, Chsc6440.DefaultI2cAddress)), new Size(display.ScreenWidth, display.ScreenHeight), interruptPin);
    touch.UpdateInterval = TimeSpan.FromMilliseconds(100);
    touch.EnableEvents();
}

while (true)
{
    float factor = i / 10.0f;
    if (Console.KeyAvailable || (_touch != null && _touch.IsPressed()))
    {
        break;
    }

    IGraphics api = backBuffer.GetDrawingApi();
    Rectangle newRect = Rectangle.Empty;
    newRect.Width = (int)(image.Width * factor);
    newRect.Height = (int)(image.Height * factor);
    newRect.X = (backBuffer.Width / 2) - (newRect.Width / 2);
    newRect.Y = (backBuffer.Height / 2) - (newRect.Height / 2);
    api.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), newRect);

    ili9341.DrawBitmap(backBuffer);
    ili9341.SendFrame();
    Thread.Sleep(100);
}
```

## Binding Notes

This binding currently only supports commands and raw data. Drawing needs to be done to a backbuffer, which then can be
forwarded to the device. See example above.

The following connection types are supported by this binding.

- [X] SPI
- [X] I2C (for the touch controller)
