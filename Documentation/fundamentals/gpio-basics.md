# GPIO Basics

General-Purpose Input/Output (GPIO) pins are the foundation of IoT hardware interfacing. This guide explains the essential concepts you need to understand when working with GPIO pins in .NET IoT.

## What is GPIO?

GPIO pins are programmable pins on your single-board computer that can:

- **Output digital signals** - Turn devices on/off (LEDs, relays, motors)
- **Read digital signals** - Detect button presses, sensor states
- **Generate PWM signals** - Control brightness, speed (requires specific pins)
- **Communicate via protocols** - I2C, SPI, UART (requires specific pins)

## Pin Numbering Systems

There are different ways to refer to pins:

### GPIO/BCM Numbering (Used by .NET IoT)

This numbering refers to the Broadcom (BCM) chip's GPIO numbers:

```csharp
// GPIO 18 refers to the BCM GPIO18 pin
controller.OpenPin(18, PinMode.Output);
```

### Physical Numbering

Physical pin numbers (1-40) refer to the position on the header. **Do not use these in .NET IoT code.**

### Example

- **GPIO 18** = **Physical Pin 12** on Raspberry Pi
- Your code uses `18`, but you wire to physical position 12

**Tip:** Use a pinout diagram for your specific board to map GPIO numbers to physical positions.

## Digital Output

Digital output means setting a pin to either HIGH (3.3V on Raspberry Pi) or LOW (0V/Ground).

### Basic Output Example

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(18, PinMode.Output);

// Turn on (HIGH = 3.3V)
controller.Write(18, PinValue.High);

// Turn off (LOW = 0V)
controller.Write(18, PinValue.Low);
```

### Driving an LED

LEDs require a current-limiting resistor to prevent damage:

```
GPIO Pin → LED (Anode/+) → LED (Cathode/-) → Resistor (220Ω) → Ground
```

**Important:** Calculate proper resistor values:

- Raspberry Pi outputs 3.3V
- Typical LED forward voltage: 2.0V
- Typical LED current: 10-20mA
- Resistor = (3.3V - 2.0V) / 0.015A ≈ 87Ω (use 220Ω for safety)

## Digital Input

Digital input means reading whether a pin is HIGH or LOW.

### Basic Input Example

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(17, PinMode.Input);

// Read current state
PinValue value = controller.Read(17);
if (value == PinValue.High)
{
    Console.WriteLine("Button is pressed");
}
```

### Pull-up and Pull-down Resistors

**Problem:** When a button is not pressed, the input pin "floats" (undefined state), causing unreliable readings.

**Solution:** Use pull-up or pull-down resistors to define the default state.

#### Pull-up Resistor

Pulls the pin to HIGH by default. Button press connects to Ground (LOW).

```csharp
controller.OpenPin(17, PinMode.InputPullUp);

// When button is NOT pressed: reads HIGH
// When button IS pressed: reads LOW (inverted logic)
if (controller.Read(17) == PinValue.Low)
{
    Console.WriteLine("Button pressed");
}
```

#### Pull-down Resistor

Pulls the pin to LOW by default. Button press connects to 3.3V (HIGH).

```csharp
controller.OpenPin(17, PinMode.InputPullDown);

// When button is NOT pressed: reads LOW
// When button IS pressed: reads HIGH (normal logic)
if (controller.Read(17) == PinValue.High)
{
    Console.WriteLine("Button pressed");
}
```

**Recommendation:** Use `InputPullUp` for buttons - it's more noise-resistant.

## Interrupt-driven Input

Instead of constantly polling (checking) input pins, use interrupts for efficient event detection:

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(17, PinMode.InputPullUp);

// Register event handler
controller.RegisterCallbackForPinValueChangedEvent(
    17,
    PinEventTypes.Falling, // Trigger on button press (HIGH → LOW)
    (sender, args) =>
    {
        Console.WriteLine($"Button pressed at {args.ChangeTime}");
    });

Console.WriteLine("Press the button. Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);
```

### Event Types

- `PinEventTypes.Rising` - LOW → HIGH transition
- `PinEventTypes.Falling` - HIGH → LOW transition
- `PinEventTypes.None` - No events

**Note:** Physical buttons bounce (generate multiple transitions). See [Debouncing](debouncing.md) for solutions.

## Current and Voltage Limitations

### Raspberry Pi GPIO Specifications

- **Output Voltage:** 3.3V (not 5V!)
- **Max current per pin:** ~16mA (safe limit)
- **Max total current (all pins):** ~50mA
- **Input voltage tolerance:** 0V to 3.3V (5V will damage the pin!)

### Connecting 5V Devices

**Never connect 5V signals directly to GPIO pins!** Use one of these solutions:

1. **Voltage divider** (for input signals)
2. **Level shifter** (bidirectional, for I2C/SPI)
3. **Transistor/MOSFET** (for high-current outputs)

### Driving High-Current Devices

For loads exceeding 16mA (motors, relays, high-power LEDs):

1. Use a transistor or MOSFET as a switch
2. Power the load from external supply
3. GPIO controls the transistor base/gate
4. Include a flyback diode for inductive loads (motors, relays)

## Pin Mode Summary

| PinMode | Description | Typical Use |
|---------|-------------|-------------|
| `Output` | Drive pin HIGH or LOW | LEDs, control signals |
| `Input` | Read pin state (floating) | Rarely used |
| `InputPullUp` | Read pin, default HIGH | Buttons (active-low) |
| `InputPullDown` | Read pin, default LOW | Sensors (active-high) |

## Best Practices

1. **Always dispose of GpioController** - Use `using` statements or call `.Dispose()`
2. **Close pins when done** - `controller.ClosePin(pin)` or dispose controller
3. **Check pin availability** - Some pins are reserved for special functions
4. **Document your pin usage** - Especially in projects using multiple pins
5. **Add safety delays** - Small delays prevent rapid toggling issues
6. **Verify voltage levels** - Measure with multimeter when unsure
7. **Use proper resistors** - Calculate values, don't guess

## Common GPIO Pins on Raspberry Pi

Not all GPIO pins are suitable for general use:

- **GPIO 2, 3:** I2C (have pull-up resistors)
- **GPIO 14, 15:** UART (default serial console)
- **GPIO 9, 10, 11:** SPI
- **GPIO 18, 19:** PWM-capable
- **Safe for general use:** 17, 22, 23, 24, 25, 27

**Tip:** Check your specific Raspberry Pi model's pinout diagram.

## Example: Traffic Light Controller

```csharp
using System.Device.Gpio;
using System.Threading;

using GpioController controller = new();

const int redPin = 17;
const int yellowPin = 27;
const int greenPin = 22;

controller.OpenPin(redPin, PinMode.Output);
controller.OpenPin(yellowPin, PinMode.Output);
controller.OpenPin(greenPin, PinMode.Output);

while (true)
{
    // Red light
    controller.Write(redPin, PinValue.High);
    Thread.Sleep(5000);
    controller.Write(redPin, PinValue.Low);

    // Yellow light
    controller.Write(yellowPin, PinValue.High);
    Thread.Sleep(2000);
    controller.Write(yellowPin, PinValue.Low);

    // Green light
    controller.Write(greenPin, PinValue.High);
    Thread.Sleep(5000);
    controller.Write(greenPin, PinValue.Low);

    // Yellow light again
    controller.Write(yellowPin, PinValue.High);
    Thread.Sleep(2000);
    controller.Write(yellowPin, PinValue.Low);
}
```

## Next Steps

- [Understanding Protocols](understanding-protocols.md) - Learn about I2C, SPI, and UART
- [Signal Debouncing](debouncing.md) - Handle noisy button inputs properly
- [Choosing Drivers](choosing-drivers.md) - libgpiod vs sysfs

## Additional Resources

- [GPIO Wikipedia](https://en.wikipedia.org/wiki/General-purpose_input/output)
- [Raspberry Pi GPIO Pinout](https://pinout.xyz/)
- [Digital I/O Fundamentals](http://www.ni.com/white-paper/3405/en/#toc1)
