# LED Displays - 5641AS, HT16K33 and LED Matrix Display Driver

A number of LED displays have supported devices or can be extended as necessary.

## HT16K33 - LED Matrix Display Driver

The [Ht16k33](https://cdn-shop.adafruit.com/datasheets/ht16K33v110.pdf) is a multi-function LED controller driver. It is used as a [backpack driver for several Adafruit products](https://www.adafruit.com/?q=Ht16k33). It uses the I2C protocol.

This binding and samples are based on [adafruit/Adafruit_CircuitPython_HT16K33](https://github.com/adafruit/Adafruit_CircuitPython_HT16K33).

### 7-Segment Display

These [bright crisp displays](https://www.adafruit.com/product/1270) are good for showing numeric output. Besides the four 7-segments there is a top right dot (perhaps useful as a degrees symbol) and two sets of colon-dots (good for time-based projects). They come in several colors.

<img src="https://cdn-shop.adafruit.com/970x728/1268-00.jpg" width ="250px" title="Adafruit 1.2 inch 4-Digit 7-Segment Display w/I2C Backpack - Green" alt="A picture of a breakout board with a 7-Segment Display" />

You can write the following code to control them or checkout a [larger sample](samples/Large4Digit7SegmentDisplay/Program.cs).

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

### 14-Segment Display

This [display](https://shop.pimoroni.com/products/four-letter-phat?variant=39256047178) is good for showing alpha-numeric output, and its additional segments provide a wider range of characters

<img src="https://shop.pimoroni.com/cdn/shop/products/Four_letter_pHAT_5_of_5_1a005b45-151c-4938-8610-8ec758b4182d_1500x1500.JPG?v=1539263861"  width ="250px" title="Pimoroni Four-Letter Phat" alt="A picture of a four-letter display" />

Checkout a [sample](samples/Large4Digit14SegmentDisplay/Program.cs).

### 8x8 and 16x8 LED Matrix

Make a [scrolling sign or a small video display](https://www.adafruit.com/product/1614) with [16x8](https://www.adafruit.com/product/2040), [8x8](https://www.adafruit.com/product/1632), and [Bicolor](https://www.adafruit.com/product/902) LED matrices. They are quite visible but not so large it won't plug into a breadboard!

![16x8 LED matrix](https://camo.githubusercontent.com/884d1a62e3ecf4f0c5d89f0b78cb38a65e0bf3955a39531ef6d55e4724191b65/68747470733a2f2f6d65646961302e67697068792e636f6d2f6d656469612f49336163613635723348325a574c696b344d2f323030772e77656270)

![8x8 Bicolor LED matrix](https://camo.githubusercontent.com/f85caa66967ebd6752469f1baff0a660104dbe02081f42f1ee78c920f4b60cdd/68747470733a2f2f6d65646961312e67697068792e636f6d2f6d656469612f3974316d38477466613841346162477443682f323030772e77656270)

You can write the following code to control them or checkout a [larger sample](samples/Matrix/Program.Matrix.cs) ([Bicolor sample](samples/Matrix8x8Bicolor/Program.Matrix8x8Bicolor.cs)).

```csharp
using Matrix8x8 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

matrix.Clear();
// Set pixel in the top left
matrix[0, 0] = 1;
// Set pixel in the middle
matrix[3, 4] = 1;
matrix[4, 3] = 1;
// Set pixel in the bottom right
matrix[7, 7] = 1;
```

### Bi-Color Bargraph Usage

Make a [small linear display](https://www.adafruit.com/product/1721) with multiple colors using this elegant bi-color LED bargraph. Every bar has two LEDs inside so you can have it display red, green, yellow or with fast multiplexing (provided by the HT16K33 driver chip) any color in between.

![Bi-Color (Red/Green) 24-Bar Bargraph w/I2C Backpack Kit](https://camo.githubusercontent.com/7667a4f1a7f3956b94c8d4373668290fa6af5cf76862553f54247dffe91b4745/68747470733a2f2f692e67697068792e636f6d2f6d656469612f326c4d71686e6b494273504d47704f4a49782f67697068792d646f776e73697a65642e676966)

You can write the following code to control them or checkout a [larger sample](samples/BiColorBargraph/Program.BiColorBargraph.cs).

```csharp
using BiColorBarGraph bargraph = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

bargraph.Clear();
bargraph[0] = LedColor.RED;
bargraph[1] = LedColor.GREEN;
bargraph[2] = LedColor.YELLOW;
bargraph[3] = LedColor.OFF;
bargraph[4] = LedColor.RED;
```

## Other Displays

### GPIO Devices

The [5641AS](http://www.xlitx.com/datasheet/5641AS.pdf) segment display is similar to above devices but without colon and degrees LEDs. It can be used without a driver.

![5641AS Segment Display](samples/LedSegmentDisplay5641AS.Sample/5641AS.jpg)

The following code initializes the pin scheme for the device - mapping pins on the device to the GPIO pins on the board - and creates the display.

```c#
var scheme = new LedSegmentDisplay5641ASPinScheme(16, 21, 6, 19, 26, 20, 5, 13,
                22, 27, 17, 4);

using var gpio = new System.Device.Gpio.GpioController();
using var display = new LedSegmentDisplay5641AS(scheme, gpio, false);
```

See the sample project for pin diagram and a stopwatch example.
