# Lp55231

The Lp55231 is a I2C controlled, Nine-Channel RGB, White-LED Driver With Internal Program Memory and Integrated Charge Pump.

## Documentation

- [Texas Instruments](https://www.ti.com/product/LP55231)
- [Sparkfun](https://www.sparkfun.com/products/13884)

## Usage

This driver currently provides just the fundamentals to drive RGB Leds. It does not currently support
* Individual control of PWM channels
* Setting LED drive current
* Programming

### RGB Leds

The following example show how to use the RGB leds on the Sparkfun Lp55231 breakout board.

```csharp
using (var device = new Lp55231();)
{
    device.Reset();

    // Give the IC time to restart
    Thread.Sleep(100);

    device.Enabled = true;

    device.Misc = MiscFlags.ClockSourceSelection
                | MiscFlags.ExternalClockDetection
                | MiscFlags.ChargeModeGainHighBit
                | MiscFlags.AddressAutoIncrementEnable;

    device.RgbLeds[0].Red = 0xFF; // Make RGBLed 0 show red
    device.RgbLeds[1].Green = 0xFF; // Make RGBLed 1 show green
    device.RgbLeds[2].Blue = 0xFF; // Make RGBLed 2 show blue
}
```
