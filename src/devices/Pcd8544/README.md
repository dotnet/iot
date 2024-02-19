# PCD8544 - 48 × 84 pixels matrix LCD, famous Nokia 5110 screen

This is the famous screen that Nokia 5110 used. It's a SPI based device. This Nokia 5110 was very popular and many of us used to have it in their hands and that young generation have seen it in pictures. This LCD is quite cheap and easy to use.

## Documentation

- Nokia5110 [datasheet](https://www.sparkfun.com/datasheets/LCD/Monochrome/Nokia5110.pdf)

## Usage

The screen is most of the time presented like this:

![Nokia 5110 screen](./Nokia-5110-LCD-Pinout.png)

It uses SPI and you can add PWM to control the 4 leds brightness which are around the screen. The pin have various names like  BL or LED.

It requires as well a GPIO controller on top of the SPI device with a pin called Data Control, most often presented as DC.

In this schema, the Chip Select (active low) is most of the time called CE. The SPI Clock has to be connected to CLK or SCLK. And the MOSI pin to DIN or MOSI. Names for the pins varies a bit.

Reset pin is sometimes called RST.

Supply voltage can vary, 3.3 or 5V, up to 7V is acceptable. Make sure that there is a current limiting resistor between the brightness led and the controller you'll plug. Most of the kits you buy have it. If you don't add one, you will damage the external brightness leds.

The following example shows how to setup the screen with a PWM to control the brightness:

```csharp
SpiConnectionSettings spiConnection = new(0, 1) { ClockFrequency = 5_000_000, Mode = SpiMode.Mode0, DataFlow = DataFlow.MsbFirst, ChipSelectLineActiveState = PinValue.Low };
PwmChannel pwmChannel = PwmChannel.Create(0, 0);
SpiDevice spi = SpiDevice.Create(spiConnection);
Pcd8544 lcd = new(27, 22, spi, pwmChannel);
```

If you don't want neither a PWM neither a reset pin, you can then pass a negative pin number for reset and null for the PWM:

```csharp
lcd = new(27, spi, -1, null);
```

Not that there is as well the possibility to pass a normal pin number for the backlight. In this case, the light will be on once the `BacklightBrightness` property if more then 0.5, otherwise off.

### Displaying text

Like for other screen bindings in this repository you have `Write` and `SetCursorPosition` to write text and set the cursor position.

A specific font is available and optimized for the screen. The font only contains a limited number of characters and will just ignore any characters that are unknown.

```csharp
lcd.SetCursorPosition(0, 0);
lcd.WriteLine("First line");
lcd.Write("Second one");
lcd.SetCursorPosition(0, 5);
lcd.Write("last line");
```

The position of the cursor moves with the text you write. The screen has 6 lines in total. And a character is 5 pixels width and 8 pixels height.

You can as well provide your own character using the `Write(ReadOnlySpan<byte> text)` function.

### Drawing lines, rectangles and points

Point, line and rectangle primitives are available:

```csharp
lcd.DrawPoint(5, 5, true);
lcd.DrawLine(0, 0, 15, 35, true);
lcd.DrawRectangle(10, 30, 10, 20, true, true);
lcd.DrawRectangle(12, 32, 6, 16, false, true);
// You should not forget to draw what you have in memory
lcd.Draw();
```

Each can take `Point`, `Size` and `Rectangle` as well as input. You have to decide if you want to have the point on or off and if you fill or not the rectangle.

Also you should `Refresh` the screen once you'll finish your drawing. All drawing are in the memory and needs to be pushed to the screen.

### Displaying raw images

The function DrawBitmap allows you to draw anything, you'll just need to provide the raw bitmap. Size is 504 bytes representing from the top left part columns of 8 pixels up to the right up to the next raw. A total of 6 raws are available with 84 columns each.

Here is an example on how you can convert an existing image to extract the raw data:

```csharp
using BitmapImage bitmapNokia = BitmapImage.CreateFromFile(Path.Combine("nokia_bw.bmp"));
lcd.DrawBitmap(bitmap2);
```

In case you want to convert existing images which have a different size than the 84x48 screen size, you have to resize the picture like this using SkiaSharp:

```csharp
// Open a non bitmap and resize it
using BitmapImage bitmapLarge = BitmapImage.CreateFromFile("nonbmp.jpg"));
var bitmap2 = bitmapLarge.Resize(Pcd8544.PixelScreenSize);
```

Note: you may want to reverse the colors first depending on what you want.

### Advanced functions

You can adjust couple of factors like `Bias`,  `Temperature`, `Contrast` and `Brightness`. The [samples](./samples/Program.cs) will run thru all of them so you can understand the impact of each of them. The `Bias` will increase the voltage and darken the screen.

In general, it is recommended to leave Temperature to the 0 coefficient, if you are in normal conditions. The Bias can be left to 4 as well as a default value for normal conditions.

Then you can use the Contrast property to properly adjust the contrast. A value around 0x30 is usually a good one for normal conditions.

But you may have to adjust those depending on the conditions of lights, temperature you are in.

#### Brightness

The brightness is controlling the PWM. If you have not passed any PWM controller, this has no effect on the screen.
Brightness goes from 0.0 to 0.1f.

```csharp
lcd.BacklightBrightness = 0.2f;
```

#### InvertedColors

An invert mode is available, it just revert the screen colors. So white pixels become black and vice versa

```csharp
lcd.InvertedColors = true;
```

#### Enabled

You can switch on or off the screen fully with the `Enabled` property:

```csharp
lcd.Enabled = true;
```
