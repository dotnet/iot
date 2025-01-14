# Blinkt - 8-LED indicator strip

## Summary

[Blinkt!](https://shop.pimoroni.com/products/blinkt) offers eight APA102 pixels in the smallest (and cheapest) form factor to plug straight onto your Raspberry Pi.

Each pixel on Blinkt! is individually controllable and dimmable allowing you to create gradients, pulsing effects, or just flash them on and off like crazy.

## Binding Notes

The Blinkt has 8 pixels that can be controlled independantly. This library is designed to mimic the functionality of the [Pimoroni Blinkt Python library](https://github.com/pimoroni/blinkt), except using System.Drawing.Color instead of separate R, G, B and brightness values. the Alpha channnel is used to set the brightness of the pixel.

Setting the pixel values does not update the display. You must call the `Show` method to update the display. This allows you to configure how you want all the pixels to look before updating the display.

## Usage

Here is an example how to use the Blinkt:

```csharp
using System.Drawing;
using Iot.Device.Blinkt;

// Create the Blinkt
var blinkt = new Blinkt();

// Set all the pixels to random colors
for (int i = 0; i < Blinkt.NUMBER_OF_PIXELS; i++)
{
    var color = Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));
    blinkt.SetPixel(i, color);
}

// Update the display to show the new colors
blinkt.Show();
```

## References

[Blinkt on Pimoroni](https://shop.pimoroni.com/products/blinkt)
