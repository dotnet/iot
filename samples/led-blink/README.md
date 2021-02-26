# Blink an LED with .NET Core on a Raspberry Pi

This [sample](Program.cs) demonstrates blinking an LED. The sample also demonstrates the most basic usage of the [.NET Core GPIO library](https://www.nuget.org/packages/System.Device.Gpio).

The following code toggles a GPIO pin on and off, which powers the LED.

```csharp
int pin = 18;
int lightTime = 1000;
int dimTime = 200;

using GpioController controller = new();
controller.OpenPin(pin, PinMode.Output);

while (true)
{
    controller.Write(pin, PinValue.High);
    Thread.Sleep(lightTime);
    controller.Write(pin, PinValue.Low);
    Thread.Sleep(dimTime);
}
```

The following [fritzing diagram](rpi-led.fzz) demonstrates how you should configure your breadboard to match the code above.

![Raspberry Pi Breadboard diagram](rpi-led_bb.png)

## Resources

* [Diffused LEDs](https://www.adafruit.com/product/297)
* [All about LEDs](https://learn.adafruit.com/all-about-leds)
- [Blinking an LED with Arduino](https://learn.adafruit.com/adafruit-arduino-lesson-2-leds/blinking-the-led)
- [Blinking an LED with Python](https://learn.adafruit.com/blinking-an-led-with-beaglebone-black/writing-a-program)