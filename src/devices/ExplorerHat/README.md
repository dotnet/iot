# Explorer HAT Pro (Pimoroni)

[Explorer HAT Pro](https://shop.pimoroni.com/products/explorer-hat) is an add-on board for Raspberry Pi.

## Documentation

* [Explorer HAT Technical Reference](https://github.com/pimoroni/explorer-hat/blob/master/documentation/Technical-reference.md)

![Explorer HAT Pro](https://user-images.githubusercontent.com/10654401/63101233-e88c4b80-bf78-11e9-87ff-20e7a2809c40.png)

It consists of multiple devices. Currently supported devices:

* Four coloured LEDs (red, green, blue, and yellow)
* Two H-bridge motor drivers (up to 200mA per channel; soft PWM control)

## Notes

Capacitive touchpad, inputs, outputs, and 3.3v breakout not supported... Working on them.

## Usage

Hat initialization:

```csharp
using (var hat = new ExplorerHat())
{
    // Your code here
}
```

### Leds

```csharp
// All lights on
hat.Lights.On();
Thread.Sleep(1000);
// All lights off
hat.Lights.Off();
Thread.Sleep(500);

// By color
hat.Lights.Blue.On();
Thread.Sleep(1000);
hat.Lights.Blue.Off();
Thread.Sleep(500);
hat.Lights.Yellow.On();
Thread.Sleep(1000);
hat.Lights.Yellow.Off();
Thread.Sleep(500);
hat.Lights.Red.On();
Thread.Sleep(1000);
hat.Lights.Red.Off();
Thread.Sleep(500);
hat.Lights.Green.On();
Thread.Sleep(1000);
hat.Lights.Green.Off();
Thread.Sleep(500);

// By number
hat.Lights.One.On();
Thread.Sleep(1000);
hat.Lights.One.Off();
Thread.Sleep(500);
hat.Lights.Two.On();
Thread.Sleep(1000);
hat.Lights.Two.Off();
Thread.Sleep(500);
hat.Lights.Three.On();
Thread.Sleep(1000);
hat.Lights.Three.Off();
Thread.Sleep(500);
hat.Lights.Four.On();
Thread.Sleep(1000);
hat.Lights.Four.Off();
Thread.Sleep(500);

// Iterate through led array
int i = 0;
foreach (var led in hat.Lights)
{
    i++;
    Console.WriteLine($"Led #{i} is {(led.IsOn ? "ON" : "OFF")}");
}
```

### Motors

```csharp
// Forwards full speed
hat.Motors.Forwards(1);
Thread.Sleep(2000);

// Backwards full speed
hat.Motors.Backwards(1);
Thread.Sleep(2000);

// Manage one motor at a time
hat.Motors.One.Forwards(1);
Thread.Sleep(2000);
hat.Motors.One.Backwards(0.6);
Thread.Sleep(2000);
hat.Motors.Two.Forwards(1);
Thread.Sleep(2000);
hat.Motors.Two.Backwards(0.6);
Thread.Sleep(2000);

// Set motors speed
hat.Motors.One.Speed = 1;
Thread.Sleep(2000);
hat.Motors.One.Speed = -0.6;
Thread.Sleep(2000);
hat.Motors.Two.Speed = 0.8;
Thread.Sleep(2000);
hat.Motors.Two.Speed = -0.75;
Thread.Sleep(2000);

// Stop motors
hat.Motors.Stop();

// Stop motors one at a time
hat.Motors.Forwards(1);
Thread.Sleep(2000);
hat.Motors.One.Stop();
Thread.Sleep(2000);
hat.Motors.Two.Stop();
```
