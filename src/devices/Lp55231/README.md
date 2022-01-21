# Lp55231 - Nine-Channel RGB, White-LED Driver

The Lp55231 is a I2C controlled, Nine-Channel RGB, White-LED Driver With Internal Program Memory and Integrated Charge Pump.

## Documentation

- [Texas Instruments](https://www.ti.com/product/LP55231)
- [Sparkfun](https://www.sparkfun.com/products/13884)

## Usage

This driver currently provides just the fundamentals to drive RGB Leds. It does not currently support

- Individual control of PWM channels
- Setting LED drive current
- Programming

### RGB Leds

The following example show how to use the RGB leds on the Sparkfun Lp55231 breakout board.

```csharp
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Lp55231.DefaultI2cAddress));

using (var device = new Lp55231(i2cDevice))
{
    device.Reset();

    // Give the IC time to restart
    Thread.Sleep(100);

    device.Enabled = true;

    device.Misc = MiscFlags.ClockSourceSelection
                | MiscFlags.ExternalClockDetection
                | MiscFlags.ChargeModeGainHighBit
                | MiscFlags.AddressAutoIncrementEnable;
                
    ledDriver[0] = Color.FromArgb(0, 255, 0, 0);
    ledDriver[1] = Color.FromArgb(0, 0, 255, 0);
    ledDriver[2] = Color.FromArgb(0, 0, 0, 255);
}
```
