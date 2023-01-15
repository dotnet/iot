# Button

The `ButtonBase` class is a base implementation for buttons that is hardware independent and can be used across devices.
The `GpioButton` is a GPIO implementation of the button and inherits from the `ButtonBase` class. This implementation has been tested on an ESP32 platform, specifically on the [M5StickC Plus](https://shop.m5stack.com/products/m5stickc-plus-esp32-pico-mini-iot-development-kit).

## Documentation

Documentation for the M5StickC Plus, including pin mapping, can be [found here](https://docs.m5stack.com/en/core/m5stickc_plus).
Information regarding standard mouse events, used as inspiration for the button events, can be [found here](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/input-mouse/events?view=netdesktop-5.0#standard-click-event-behavior).

## Configuring the pull-ups or pull-downs for the button

A general rule in electronics is that input pins not permanently tied to a wire whose state is `LOW` or `HIGH`, should always be pulled up or down with a resistor.
For example, the button causes the input GPIO to have a stable state only during the pressed state.
In order to ensure the released state to be either `LOW` or `HIGH`, a resistor must always be either added or configured in software to avoid unpredictable readings.

Many boards supports configuring an internal resistor but they may not be available in for all the GPIOs. The developer should carefully read the board documentation to verify whether the internal resistor is supported or not for the specific GPIO.

`PinMode` is an enumeration used to specify whether the GPIO will be used as output or input. In the latter case it can be configured
without resistor or with a resistor configured either as a pull-up or pull-down.

Since this library detects the transitions using pin state changed event, the button push is detected differently when the button is
normally HIGH or LOW which in turns depend on the resistor configuration.

In the following table, the first two column represents the physical connections of the two button pins. The last two columns are the values that should be specified in the `Button` constructor.

| Button Pin 1<br />(connected to the GPIO) | Button Pin 2 | `IsExternalResistor` | `PinMode`        |
| ----------------------------------------- | ------------ | -------------------- | ---------------- |
| Resistor to `Vcc`                         | `GND`        | True                 | `Input.PullUp`   |
| Resistor to `GND`                         | `Vcc`        | True                 | `Input.PullDown` |
| -                                         | `GND`        | False                | `Input.PullUp`   |
| -                                         | `Vcc`        | False                | `Input.PullDown` |

> The `PinMode.Input` value should never be specified. This binding does not enforce this validation to not break the code written before the introduction of the `IsExternalResistor` argument.

## Usage

You can find an example in the [samples](./Samples/Program.cs) directory.

```csharp
// Initialize a new button with the corresponding button pin
GpioButton button = new GpioButton(buttonPin: 37);

Debug.WriteLine("Button is initialized, starting to read state");

// Enable or disable holding or doublepress events
button.IsDoublePressEnabled = true;
button.IsHoldingEnabled = true;

// Write to debug if the button is down
button.ButtonDown += (sender, e) =>
{
    Debug.WriteLine($"buttondown IsPressed={button.IsPressed}");
};

// Write to debug if the button is up
button.ButtonUp += (sender, e) =>
{
    Debug.WriteLine($"buttonup IsPressed={button.IsPressed}");
};

// Write to debug if the button is pressed
button.Press += (sender, e) =>
{
    Debug.WriteLine($"Press");
};

// Write to debug if the button is double pressed
button.DoublePress += (sender, e) =>
{
    Debug.WriteLine($"Double press");
};

// Write to debug if the button is held and released
button.Holding += (sender, e) =>
{
    switch (e.HoldingState)
    {
        case ButtonHoldingState.Started:
            Debug.WriteLine($"Holding Started");
            break;
        case ButtonHoldingState.Completed:
            Debug.WriteLine($"Holding Completed");
            break;
    }
};

Thread.Sleep(Timeout.Infinite);
```

### Expected output

```console
Button is initialized, starting to read state
buttondown IsPressed=True
buttonup IsPressed=False
Press
buttondown IsPressed=True
buttonup IsPressed=False
Press
Double press
buttondown IsPressed=True
Holding Started
buttonup IsPressed=False
Press
Holding Completed
```

## Testing

The unit test project can be found in the [tests](./Tests/ButtonTests.cs) directory. You can simply run them using the VS2019 built-in test capabilites:

![unit tests](./unittests.png)
