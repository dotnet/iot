# Button

The `ButtonBase` class is a base implementation for buttons that is hardware independent and can be used across devices.
The `GpioButton` is a GPIO implementation of the button and inherits from the `ButtonBase` class. This implementation has been tested on an ESP32 platform, specifically on the [M5StickC Plus](https://shop.m5stack.com/products/m5stickc-plus-esp32-pico-mini-iot-development-kit).

## Documentation

Documentation for the M5StickC Plus, including pin mapping, can be [found here](https://docs.m5stack.com/en/core/m5stickc_plus).
Information regarding standard mouse events, used as inspiration for the button events, can be [found here](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/input-mouse/events?view=netdesktop-5.0#standard-click-event-behavior).

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
