# Segment display driver (HT16K33)

This project contains multipurpose LED display drivers and binding implementations for concrete display configurations.

## Device family

The **HT16K33** is LED display driver that supports multiple LED configurations and I2C communication.

Adafruit sells multiple display backpacks built upon this driver:

[Adafruit LED / SEGMENTED category](https://www.adafruit.com/category/103)

**Large4Digit7SegmentDisplay** is a binding that supports the **Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack** that comes in 3 colors:

[Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack - Yellow](https://www.adafruit.com/product/1268)

[Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack - Green](https://www.adafruit.com/product/1269)

[Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack - Red](https://www.adafruit.com/product/1270)

More information on wiring can be found on the respective product pages.

## Usage

```csharp
// Initialize display (busId = 1 for Raspberry Pi 2 & 3)
var display = new Large4Digit7SegmentDisplay(I2cDevice.Create(new I2cConnectionSettings(busId: 1, address: Ht16k33.DefaultI2cAddress));

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

// Dispose display object (the device itself will not be turned off until powered down)
display.Dispose();
```

## Documentation

[HT16K33 datasheet](https://cdn-shop.adafruit.com/datasheets/ht16K33v110.pdf)
