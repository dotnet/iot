# IS31FL3731 -- LED Matrix Display Driver

The [IS31FL3731](https://cdn-learn.adafruit.com/assets/assets/000/030/994/original/31FL3731.pdf). It is a compact LED driver for 144 single LEDs. The device can be programmed via an I2C compatible interface.

This binding and samples are based on [adafruit/Adafruit_CircuitPython_IS31FL3731](https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731).

## Adafruit LED Charlieplexed Matrix - 16x9

Adafruit sells [16x9 Charlieplexed LED matrices (multiple colors)](https://www.adafruit.com/product/2948) that are designed to match with the [Adafruit 16x9 Charlieplexed PWM LED Matrix Driver - IS31FL3731](https://www.adafruit.com/product/2946).

<img src="https://cdn-shop.adafruit.com/970x728/2948-05.jpg" width="250px" alt="16x9 Charlieplexed LED matrix" />

You can write the following code to control them or checkout a [larger sample](samples/Matrix/Program.cs).

```csharp
using Backpack16x9 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31Fl3731.DefaultI2cAddress)));

matrix.Initialize();
matrix.EnableBlinking(0);
matrix.Fill(0);

matrix.Clear();
// Set pixel in the top left
matrix[0, 0] = 1;
// Set pixel in the middle
matrix[7, 3] = 1;
matrix[8, 4] = 1;
// Set pixel in the bottom right
matrix[15, 8] = 1;
```

## Adafruit Charlieplexed LED Matrix Bonnet - 16x8

You won't be able to look away from the mesmerizing patterns created by this [Adafruit CharliePlex LED Matrix Display Bonnet](https://www.adafruit.com/product/4122). This 16x8 LED display can be placed atop any Raspberry Pi computer with a 2x20 connector, for a beautiful, bright grid of 128 charlieplexed LEDs. It even comes with a built-in charlieplex driver that is run over I2C.

<img src="https://cdn-shop.adafruit.com/970x728/4122-00.jpg" width="250px" alt="Adafruit Charlieplexed LED Matrix Bonnet - 16x8" />

You can write the following code to control them or checkout a [larger sample](samples/Matrix/Program.cs).

```csharp
using Bonnet16x8 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31Fl3731.DefaultI2cAddress)));

matrix.Initialize();
matrix.EnableBlinking(0);
matrix.Fill(0);

matrix.Clear();
// Set pixel in the top left
matrix[0, 0] = 1;
// Set pixel in the middle
matrix[7, 3] = 1;
matrix[8, 4] = 1;
// Set pixel in the bottom right
matrix[15, 7] = 1;
```

## Pimoroni Scroll pHAT HD -- 17x7

A whole heap of LED pixels with individual brightness control, for scrolling messages, animations, and more. [Scroll pHAT HD](https://shop.pimoroni.com/products/scroll-phat-hd) packs 17x7 pixels (119 total) onto a single pHAT, and gives you full PWM brightness control over each pixel. Create beautiful animations and even anti-aliased text by taking advantage of the per-pixel brightness.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/scroll-phat-hd-colours-red_768x768.jpg?v=1519637267" width="250px" alt="Pimoroni Scroll pHAT HD -- 17x7" />

You can write the following code to control them or checkout a [larger sample](samples/Matrix/Program.cs).

```csharp
using ScrollHat17x7 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x61)));;

matrix.Initialize();
matrix.EnableBlinking(0);
matrix.Fill(0);

matrix.Clear();
// Set pixel in the top left
matrix[0, 0] = 1;
// Set pixel in the middle
matrix[7, 3] = 1;
matrix[8, 4] = 1;
// Set pixel in the bottom right
matrix[16, 8] = 1;
```

The same code works for [Scroll HAT Mini](https://shop.pimoroni.com/products/scroll-hat-mini).

## Pimori LED Shim

28 tiny RGB LED pixels in a single row that just slip right onto your Pi's pins, no soldering required! [LED SHIM](https://shop.pimoroni.com/products/led-shim) is ideal for status updates, notifications, a VU meter, or as a bar graph for sensor readings.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/led-shim-medium-4_1500x1500.jpg" width="250px" alt="Pimori LED Shim" />

You can write the following code to control them or checkout a [larger sample](samples/LedShim28x1/Program.cs).

```csharp
using LedShim28x1 shim = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x75)));
shim.Initialize();
shim.EnableBlinking(0);
shim.Fill(0);

for (int y = 0; y < 3; y++)
{
    for (int x = 0; x < shim.Width; x++)
    {
        shim[x, y] = 0xff;
        Thread.Sleep(100);
        shim[x, y] = 0;
    }
}
```

## Pimori Led Matrix Breakout - 11x7

A tiny, tightly-packed matrix of 77 individually-controllable bright white LEDs. Ideal for scrolling text, animations, bar graphs, or just general illumination, and it's Raspberry Pi and Arduino-compatible.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/11x7_matrix_breakout_1_of_4_1500x1500.JPG" width="250px" alt="Pimori Led Matrix Breakout - 11x7" />

You can write the following code to control them or checkout a [larger sample](samples/Matrix/Program.cs).

```csharp
using Breakout11x7 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x75)));

matrix.Initialize();
matrix.EnableBlinking(0);
matrix.Fill(0);

matrix.Clear();
// Set pixel in the top left
matrix[0, 0] = 1;
// Set pixel in the middle
matrix[4, 3] = 1;
matrix[5, 4] = 1;
// Set pixel in the bottom right
matrix[10, 6] = 1;
```
