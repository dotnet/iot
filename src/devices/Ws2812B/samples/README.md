# Drive Neo pixel strip (x8) from Raspberry Pi

This [program](Ws2812b.Sample.cs) demonstrates how to use the [Neo pixel binding](../Ws2812b.cs) to drive an 8 Neo Pixel stick from a Raspberry Pi.

It shows how to set the colors of each LED using `SetPixel` and `Update`. Then it shows how to fade in one of the LEDs in a loop.

## Run the sample

```console
cd samples
dotnet build -c release -o out
dotnet out/Ws2812b.Samples.dll
```

## Breadboard layout

The following [fritzing diagram](rpi-neo-pixels.fzz) demonstrates how you should wire your device in order to run the program. It uses the GND, 5V and MOSI pins on the Raspberry Pi.

![Raspberry Pi Breadboard diagram](rpi-neo-pixels_bb.png)

## Hardware elements

The following elements are used in this sample:

* [Raspberry Pi 3](https://www.adafruit.com/product/3055)
* [Neo pixels x8 stick](https://www.adafruit.com/product/1426)
