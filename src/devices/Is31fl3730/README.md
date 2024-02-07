# IS31FL3730 -- LED Matrix Display Driver

The [IS31FL3730](https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf) is a compact LED driver that can drive one or two 8×8, 7×9, 6×10, or 5×11 dot matrix displays. The device can be programmed via an I2C compatible interface.

This binding is similar to [pimoroni/microdot-phat](https://github.com/pimoroni/microdot-phat), a Python implementation provided by Pimoroni.

It is demonstrated in [samples/Program.cs](samples/Program.cs).

## Micro Dot pHAT

The [Micro Dot pHat](https://shop.pimoroni.com/products/microdot-phat) is an unashamedly old school LED matrix display board, made up of six LED matrices each 5x7 pixels (for an effective display area of 30x7) plus a decimal point, using the beautiful little Lite-On LTP-305 matrices.

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/Microdot_pHAT_1_of_7_768x768.JPG" width="250px" title="Micro Dot pHat" alt="A picture of a LED matrix" />

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

<img src="https://cdn.shopify.com/s/files/1/0174/1800/products/Microdotbreakout_6of6_768x768.jpg?" width="250px" title="LED Dot Matrix Breakout" alt="A picture of a LED Dot Matrix display" />

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

## Multiple LED Breakout Units

The [LED Dot Matrix Breakout](https://shop.pimoroni.com/products/led-dot-matrix-breakout) includes two matrices and supports using up to three of the breakouts together giving you six matrices to drive. It's straightforward to tie them all together into a single logical matrix. You can do with that with the [`DotMatrix`](DotMatrix.cs) class, as demonstrated in the following sample. Alternatively, you can use [`MicroDotPhat30x7`](MicroDotPhat30x7.cs) class if you have three breakouts and follow the same ordering of I2C addresses.

<img src="https://user-images.githubusercontent.com/2608468/208778976-7a18932e-a83f-4d6e-b655-3585903393d4.png" width="250px" title="Three LED Dot Matrix Breakouts" alt="A picture of several breakout boards with Dot Matrix displays" />

```csharp
using I2cDevice firstI2c = I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x61));
using I2cDevice secondI2c = I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x62));
using I2cDevice thirdI2c = I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x63));

Is31fl3730 first = DotMatrix.InitializeI2c(firstI2c);
Is31fl3730 second = DotMatrix.InitializeI2c(secondI2c);
Is31fl3730 third = DotMatrix.InitializeI2c(thirdI2c);

DotMatrix5x7[] matrices = new DotMatrix5x7[]
{
    first[1],
    first[0],
    second[1],
    second[0],
    third[1],
    third[0],
};

DotMatrix matrix = new(matrices);

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

> The default I2C address is 0x61. You can change this to 0x63 by cutting the trace on the back of the breakout. If you cut the trace and solder the bridge the address will be 0x62 - so it's possible to use up to three of these breakouts at the same time.

That's from the [LED Dot Matrix Breakout](https://shop.pimoroni.com/products/led-dot-matrix-breakout) product page.

Said slightly differently:

- The default address is `0x61`
- Cut the bridge of `ADDR2` to change the I2C address to `0x63`
- Cut the bridge of `ADDR2` and bridge the two pads of `ADDR1` (with solder) to change the I2C address to `0x62`.
