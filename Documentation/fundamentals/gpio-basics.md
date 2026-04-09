# GPIO Basics

General-Purpose Input/Output (GPIO) pins are the foundation of IoT hardware interfacing. They are programmable electrical pins on single-board computers like the Raspberry Pi that let your .NET code interact with physical hardware — turning LEDs on and off, reading button presses, and communicating with sensors.

This guide explains the essential concepts you need to understand when working with GPIO pins in .NET IoT.

## What is GPIO?

GPIO pins are programmable pins on your single-board computer that can:

- **Output digital signals** — Turn devices on/off (LEDs, relays, motors)
- **Read digital signals** — Detect button presses, sensor states
- **Generate PWM signals** — Control brightness, speed (requires specific PWM-capable pins)
- **Communicate via protocols** — I2C, SPI, UART (requires specific pins)

GPIO pins operate with digital logic levels (HIGH/LOW):

- **HIGH** = 3.3 V on Raspberry Pi (varies by platform)
- **LOW** = 0 V (Ground)

## Pin Numbering

There are two ways to refer to pins, which can be confusing at first:

| Scheme | Example | Description |
| --- | --- | --- |
| **GPIO / BCM** | `18` | The number used by the Broadcom chip — **this is what .NET IoT uses** |
| **Physical / Board** | `12` | The physical position on the 40-pin header |

GPIO 18 and physical pin 12 are the **same pin** — just identified differently. Your code always uses **GPIO (BCM) numbers**.

Use the interactive diagram at [pinout.xyz](https://pinout.xyz/) to look up the mapping for your board.

## Digital Output

Digital output means setting a pin to either HIGH (3.3 V) or LOW (0 V). That is all digital output does: voltage on or voltage off.

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(18, PinMode.Output);

controller.Write(18, PinValue.High); // 3.3 V — turns the LED on
controller.Write(18, PinValue.Low);  // 0 V   — turns the LED off
```

You can use this to drive LEDs, buzzers, relays, and any component that responds to a high/low signal.

### Driving an LED

LEDs require a current-limiting resistor to prevent damage:

```text
GPIO Pin ──► LED (Anode/+) ──► LED (Cathode/−) ──► Resistor (220 Ω) ──► Ground
```

> **Why a resistor?** LEDs need very little current. Without a resistor, the LED draws too much current and can burn out or damage your Pi. A 220 Ω resistor limits the current to a safe level (~6 mA).

## Digital Input

Digital input means reading whether a pin is HIGH or LOW.

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(17, PinMode.InputPullUp);

PinValue value = controller.Read(17);
if (value == PinValue.Low)
{
    Console.WriteLine("Button is pressed!");
}
```

### Pull-Up and Pull-Down Resistors

When a button is **not** pressed, the pin is not connected to anything. In plain `Input` mode, this leaves the pin "floating" — its voltage drifts randomly, giving unreliable readings.

Pull-up and pull-down resistors solve this by defining a default voltage:

| Mode | Default state (not pressed) | Pressed state | Wiring |
| --- | --- | --- | --- |
| `InputPullUp` | High (3.3 V) | Low (0 V) | Button connects pin to Ground |
| `InputPullDown` | Low (0 V) | High (3.3 V) | Button connects pin to 3.3 V |

`InputPullUp` is the most common choice for buttons. The Raspberry Pi has built-in pull-up resistors that are activated when you use this mode — no external resistor required.

```csharp
controller.OpenPin(17, PinMode.InputPullUp);

// When button is NOT pressed: reads HIGH
// When button IS pressed: reads LOW (inverted logic)
if (controller.Read(17) == PinValue.Low)
{
    Console.WriteLine("Button pressed");
}
```

### Button Wiring (with InputPullUp)

```text
GPIO 17 ──► one leg of button
GND     ──► other leg of button
```

When the button is pressed, GPIO 17 gets connected to Ground (Low). When released, the internal pull-up resistor pulls it back to High.

## Interrupt-Driven Input (Events)

Instead of checking the pin over and over in a loop (polling), you can register a callback that fires when the pin's value changes. This is more efficient and more responsive:

```csharp
using System.Device.Gpio;

using GpioController controller = new();
controller.OpenPin(17, PinMode.InputPullUp);

controller.RegisterCallbackForPinValueChangedEvent(
    17,
    PinEventTypes.Falling, // trigger on button press (HIGH → LOW)
    (sender, args) =>
    {
        Console.WriteLine($"Button pressed on pin {args.PinNumber}");
    });

Console.WriteLine("Press the button. Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);
```

**Event types:**

- `PinEventTypes.Rising` — LOW → HIGH transition
- `PinEventTypes.Falling` — HIGH → LOW transition
- `PinEventTypes.Rising | PinEventTypes.Falling` — both directions

> **Note:** Physical buttons "bounce" — a single press can generate several rapid transitions. If you experience duplicate button events, you need debouncing. A simple approach is to ignore events that arrive within a short time window (e.g., 50 ms) of the previous event.

## Current and Voltage Limitations

### Raspberry Pi GPIO Specifications

- **Output Voltage:** 3.3 V (not 5 V!)
- **Max current per pin:** ~16 mA (safe limit)
- **Max total current (all pins):** ~50 mA
- **Input voltage tolerance:** 0 V to 3.3 V (**5 V will damage the pin!**)

### Connecting 5 V Devices

**Never connect 5 V signals directly to GPIO pins!** Use one of these solutions:

1. **Voltage divider** (for input signals)
2. **Level shifter** (bidirectional, for I2C/SPI)
3. **Transistor/MOSFET** (for high-current outputs)

### Driving High-Current Devices

For loads exceeding 16 mA (motors, relays, high-power LEDs), use a transistor or MOSFET as a switch powered from an external supply. The GPIO pin controls the transistor's gate, not the device directly.

## Pin Mode Summary

| PinMode | Description | Typical Use |
| --- | --- | --- |
| `Output` | Drive pin HIGH or LOW | LEDs, control signals |
| `Input` | Read pin state (floating) | Rarely used |
| `InputPullUp` | Read pin, default HIGH | Buttons (active-low) |
| `InputPullDown` | Read pin, default LOW | Sensors (active-high) |

## Common GPIO Pins on Raspberry Pi

Not all GPIO pins are suitable for general use:

- **GPIO 2, 3:** I2C (have hardware pull-up resistors)
- **GPIO 14, 15:** UART (default serial console)
- **GPIO 9, 10, 11:** SPI
- **GPIO 18, 19:** PWM-capable
- **Safe for general use:** 17, 22, 23, 24, 25, 27

**Tip:** Check your specific Raspberry Pi model's pinout at [pinout.xyz](https://pinout.xyz/).

## Best Practices

1. **Always dispose of GpioController** — Use `using` statements or call `.Dispose()`
2. **Use `InputPullUp` or `InputPullDown`** — Never leave input pins floating
3. **Respect voltage levels** — Raspberry Pi GPIO operates at 3.3 V; connecting 5 V **will damage the pin**
4. **Use resistors with LEDs** — Calculate the value or use 220 Ω as a safe default
5. **Check pin assignments** — Some GPIO pins are shared with I2C, SPI, or UART; using them for general GPIO disables those interfaces
6. **Document your pin usage** — Especially in projects using multiple pins

## Next Steps

- [GPIO Protocol Guide](../protocols/gpio.md) — Quick start, GpioController usage, drivers, and troubleshooting
- [Choosing the Right Driver](choosing-drivers.md) — LibGpiodDriver vs SysFsDriver comparison

## Additional Resources

- [GPIO Wikipedia](https://en.wikipedia.org/wiki/General-purpose_input/output)
- [Raspberry Pi GPIO Pinout](https://pinout.xyz/)
- [Digital I/O Fundamentals](http://www.ni.com/white-paper/3405/en/#toc1)
