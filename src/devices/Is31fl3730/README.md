# IS31FL3730 -- LED Matrix Display Driver

The [IS31FL3730](https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf) is a compact LED driver that can drive one or two 8×8, 7×9, 6×10, or 5×11 dot matrix displays. The device can be programmed via an I2C compatible interface.

This binding is similar to the Python version in [pimoroni/microdot-phat](https://github.com/pimoroni/microdot-phat).

It is demonstrated in [samples/Program.cs](samples/Program.cs).

## Micro Dot pHAT

The [Micro Dot pHat](https://shop.pimoroni.com/products/microdot-phat) is an unashamedly old school LED matrix display board, made up of six LED matrices each 5x7 pixels (for an effective display area of 30x7) plus a decimal point, using the beautiful little Lite-On LTP-305 matrices.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/Microdot_pHAT_1_of_7_768x768.JPG" width="250px" title="Micro Dot pHat" />

The following code demonstrates how to control the Micro Dot pHAT.


```csharp
using I2cDevice first = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.Addresses[0]));
using I2cDevice second = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.Addresses[1]));
using I2cDevice third = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.Addresses[2]));
MicroDotPhat30x7 matrix = new(first, second, third);

matrix.Fill(0);

matrix[0, 0] = 1;
matrix[0, 6] = 1;
matrix[29, 0] = 1;
matrix[29, 6] = 1;
Thread.Sleep(500);

matrix.Fill(255);
Thread.Sleep(1000);

matrix.Fill(0);
```

## LED Dot Matrix Breakout

The [LED Dot Matrix Breakout](https://shop.pimoroni.com/products/led-dot-matrix-breakout) is perfect for readouts that involve two numbers or letters - like countdown timers, percentage readouts, or country codes.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/Microdotbreakout_6of6_768x768.jpg?" width="250px" title="LED Dot Matrix Breakout" />

The following code demonstrates how to control the LED Dot Matrix Breakout.

```csharp
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, DotMatrix10x7.DefaultI2cAddress));
DotMatrix10x7 matrix = new(i2cDevice);

matrix[0, 0] = 1;
matrix[0, 6] = 1;
matrix[9, 0] = 1;
matrix[9, 6] = 1;
Thread.Sleep(500);

matrix.Fill(255);
Thread.Sleep(1000);

matrix.Fill(0);
```
