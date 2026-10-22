# Explorer HAT Pro (Pimoroni)

[Explorer HAT Pro](https://shop.pimoroni.com/products/explorer-hat) is an add-on board for Raspberry Pi.

## Documentation

* [Explorer HAT Technical Reference](https://github.com/pimoroni/explorer-hat/blob/master/documentation/Technical-reference.md)

![Explorer HAT Pro](https://user-images.githubusercontent.com/10654401/63101233-e88c4b80-bf78-11e9-87ff-20e7a2809c40.png)

It consists of multiple devices. Currently supported devices:

* Four coloured LEDs (red, green, blue, and yellow)
* Two H-bridge motor drivers (up to 200mA per channel; soft PWM control)
* Four buffered 5V tolerant inputs
* Four open collector 5V outputs (up to 500mA in total across the four outputs)

## Notes

Capacitive touchpads, analog inputs, and 3.3v breakout not supported yet.

The pins of the inputs and outputs are opened the first time they are used, not when `ExplorerHat` is created. Inputs and outputs that are not used can be used by other bindings, with their `Pin` property (for example, an HC-SR04 distance sensor with its trigger on an output and its echo on an input). When `ExplorerHat` is disposed, it switches off the outputs it used and closes only the pins it opened.

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

### Inputs

The inputs are behind a buffer that protects the Raspberry Pi from 5V signals, so the internal pull-up and pull-down resistors of the Raspberry Pi have no effect.

| Input | GPIO |
|---|---|
| `hat.Inputs.One` | 23 |
| `hat.Inputs.Two` | 22 |
| `hat.Inputs.Three` | 24 |
| `hat.Inputs.Four` | 25 |

```csharp
// Read one input
PinValue value = hat.Inputs.One.Read();
Console.WriteLine($"Input 1 is {value}");

// Read all the inputs
foreach (var input in hat.Inputs)
{
    Console.WriteLine($"Input on GPIO {input.Pin} is {input.Read()}");
}
```

### Outputs

The outputs are open collector: when an output is on, it connects the output to ground (it sinks current). Connect the device between a positive supply (for example the 5V pin of the HAT) and the output. When the output is off, it is left floating: it is not driven high. To get a logic signal from an output, add a pull-up resistor between the output and the supply voltage.

| Output | GPIO |
|---|---|
| `hat.Outputs.One` | 6 |
| `hat.Outputs.Two` | 12 |
| `hat.Outputs.Three` | 13 |
| `hat.Outputs.Four` | 16 |

```csharp
// Switch on output 1 for one second
hat.Outputs.One.On();
Thread.Sleep(1000);
hat.Outputs.One.Off();

// Check the state of an output
Console.WriteLine($"Output 1 is {(hat.Outputs.One.IsOn ? "ON" : "OFF")}");
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
