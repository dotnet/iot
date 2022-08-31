# HT16K33 -- LED Matrix Display Driver

The [Ht16k33](https://cdn-shop.adafruit.com/datasheets/ht16K33v110.pdf)  is a memory mapping and multi-function LED controller driver. It is used as a [backpack driver for several Adafruit products](https://www.adafruit.com/?q=Ht16k33). It supports multiple LED configurations and I2C communication.

Adafruit sells multiple display backpacks built upon this driver:

- [1.2" 4-Digit 7-Segment Display w/I2C Backpack - Yellow](https://www.adafruit.com/product/1268)
- [1.2" 4-Digit 7-Segment Display w/I2C Backpack - Green](https://www.adafruit.com/product/1269)
- [1.2" 4-Digit 7-Segment Display w/I2C Backpack - Red](https://www.adafruit.com/product/1270)
- [Bi-Color (Red/Green) 24-Bar Bargraph w/I2C Backpack Kit](https://www.adafruit.com/product/1721)

More information on wiring can be found on the respective product pages and at [adafruit/Adafruit_CircuitPython_HT16K33
](https://github.com/adafruit/Adafruit_CircuitPython_HT16K33) (Adafruit-maintained Python bindings).

## 7-Segment Display Usage

![Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack - Green](https://cdn-shop.adafruit.com/970x728/1268-00.jpg)

```csharp
// Initialize display (busId = 1 for Raspberry Pi 2 & 3)
using var display = new Large4Digit7SegmentDisplay(I2cDevice.Create(new I2cConnectionSettings(busId: 1, address: Ht16k33.DefaultI2cAddress));

// Set max brightness (automatically turns on display)
display.Brightness = display.MaxBrightness;

// Write time to the display
display.Write(DateTime.Now.ToString("H:mm").PadLeft(5));

// Wait 5 seconds
Thread.Sleep(5000);

// Turn on buffering
display.BufferingEnabled = true;

// Write -42°C to display using "decimal point" between 3rd and 4th digit as the ° character
display.Write("-42C");
display.Dots = Dot.DecimalPoint;

// Send buffer to the device
display.Flush();
```

## Bi-Color Bargraph Usage

![Bi-Color (Red/Green) 24-Bar Bargraph w/I2C Backpack Kit](https://cdn-shop.adafruit.com/970x728/1721-00.jpg)

```csharp
using BiColorBarGraph bargraph = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

bargraph.Clear();

bargraph[0] = BarColor.RED;
bargraph[1] = BarColor.GREEN;
bargraph[2] = BarColor.YELLOW;
bargraph[3] = BarColor.OFF;
bargraph[4] = BarColor.RED;
```
