# GPIO (General-Purpose Input/Output)

General-Purpose Input/Output (GPIO) pins are programmable pins on your single-board computer that can be used to interface with external devices. This guide covers how to get started with GPIO in .NET IoT, including a quick-start example, the `GpioController` API, driver selection, and troubleshooting.

## What You Need

- A **Linux single-board computer with GPIO headers** and libgpiod installed (for example, a Raspberry Pi 3, 4, or 5 running Raspberry Pi OS)
- **.NET 8.0 SDK** or later installed ([installation guide](https://learn.microsoft.com/dotnet/core/install/linux-debian))
- A **breadboard**, a few **jumper wires**, an **LED**, and a **220 Ω resistor**

> **Tip:** If this is your very first electronics project, search for "breadboard basics" — a breadboard lets you connect components without soldering.

## Quick Start: Blink an LED

### 1. Create a new project

```bash
dotnet new console -n LedBlink
cd LedBlink
dotnet add package System.Device.Gpio
```

### 2. Write the code

Replace the contents of `Program.cs`:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

const int ledPin = 18; // GPIO 18 = physical pin 12 on Raspberry Pi (see https://pinout.xyz/)

using GpioController controller = new();

controller.OpenPin(ledPin, PinMode.Output);

Console.WriteLine("Blinking LED. Press Ctrl+C to stop.");

while (true)
{
    controller.Write(ledPin, PinValue.High); // LED on
    Thread.Sleep(1000);
    controller.Write(ledPin, PinValue.Low);  // LED off
    Thread.Sleep(1000);
}
```

### 3. Wire the circuit

Connect these components on your breadboard:

1. **GPIO 18** (physical pin 12) → long leg (anode, +) of the LED
2. Short leg (cathode, −) of the LED → one end of a **220 Ω resistor**
3. Other end of resistor → **Ground** (physical pin 6)

```text
GPIO 18 ──► LED(+) ──► LED(−) ──► 220 Ω Resistor ──► GND
```

> **Why a resistor?** LEDs need very little current. Without a resistor, the LED draws too much current and can burn out or damage your Pi. A 220 Ω resistor limits the current to a safe level.

### 4. Run

```bash
dotnet run
```

You should see the LED blink once per second.

## Pin Numbering

.NET IoT uses **BCM (Broadcom) GPIO numbering**, not physical pin numbers. For example, GPIO 18 corresponds to physical pin 12 on a Raspberry Pi — they are the same pin, just identified differently. Always use GPIO/BCM numbers in your code.

For a complete mapping, see [pinout.xyz](https://pinout.xyz/).

## Digital Input

Digital input is the counterpart to digital output: instead of driving a pin HIGH or LOW, you **read** the voltage present on a pin. This lets you detect external signals — most commonly a button press.

To read a pin, open it in an input mode and call `Read`:

```csharp
controller.OpenPin(17, PinMode.InputPullUp);

PinValue value = controller.Read(17);
if (value == PinValue.Low)
{
    Console.WriteLine("Button pressed");
}
```

A typical button circuit connects the GPIO pin to Ground through the button. When the button is pressed the pin sees LOW; when released the internal pull-up resistor pulls it back to HIGH.

```text
GPIO 17 ──► one leg of button
GND     ──► other leg of button
```

### Why InputPullUp?

When a button is **not** pressed, the pin is not connected to anything. In plain `Input` mode, this leaves the pin "floating" — its voltage drifts randomly, giving unreliable readings. `InputPullUp` activates an internal resistor that holds the pin at HIGH by default, so the only way it goes LOW is when the button physically connects it to Ground. `InputPullDown` works the opposite way (default LOW, button connects to 3.3 V).

See [GPIO Basics](../fundamentals/gpio-basics.md) for a detailed explanation of pull-up and pull-down resistors.

## Pin Modes

GPIO pins can be configured in different modes:

```csharp
// Output — drive pin HIGH or LOW
controller.OpenPin(18, PinMode.Output);

// Input with pull-up resistor (default HIGH, button press = LOW)
controller.OpenPin(17, PinMode.InputPullUp);

// Input with pull-down resistor (default LOW, button press = HIGH)
controller.OpenPin(17, PinMode.InputPullDown);
```

## Reading and Writing

```csharp
// Write to output pin
controller.Write(18, PinValue.High);
controller.Write(18, PinValue.Low);

// Read from input pin
PinValue value = controller.Read(17);
if (value == PinValue.Low)
{
    Console.WriteLine("Button pressed");
}
```

## Interrupt-Driven Input (Events)

Instead of continuously polling, use events for efficient input handling:

```csharp
controller.RegisterCallbackForPinValueChangedEvent(
    17,
    PinEventTypes.Falling, // trigger on HIGH → LOW (button press)
    (sender, args) =>
    {
        Console.WriteLine($"Button pressed on pin {args.PinNumber}");
    });

Console.WriteLine("Press button. Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);
```

**Event types:**

- `PinEventTypes.Rising` — LOW → HIGH transition
- `PinEventTypes.Falling` — HIGH → LOW transition
- `PinEventTypes.Rising | PinEventTypes.Falling` — both transitions

> **Note:** Physical buttons "bounce" — a single press can generate several rapid transitions. If you experience duplicate button events, you need debouncing. A simple approach is to ignore events that arrive within a short time window (e.g., 50 ms) of the previous event.

## Complete Example: Button-Controlled LED

This example turns an LED on while a button is held down:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

const int buttonPin = 17;
const int ledPin = 18;

using GpioController controller = new();

controller.OpenPin(ledPin, PinMode.Output);
controller.OpenPin(buttonPin, PinMode.InputPullUp);

Console.WriteLine("Hold the button to light the LED. Ctrl+C to exit.");

while (true)
{
    // Button pressed (Low) → turn LED on (High)
    // Button released (High) → turn LED off (Low)
    PinValue buttonState = controller.Read(buttonPin);
    controller.Write(ledPin, buttonState == PinValue.Low ? PinValue.High : PinValue.Low);
    Thread.Sleep(50);
}
```

## GpioController and Drivers

The `GpioController` class is the main entry point for GPIO operations. It abstracts the underlying platform-specific implementation through **GPIO drivers**. You usually do not need to think about this — the parameterless constructor automatically picks the best driver:

```csharp
// Recommended — auto-detects the best driver for your board
using GpioController controller = new();
```

### Available Drivers

| Driver | When to use |
| --- | --- |
| `LibGpiodDriver` | Modern Linux with libgpiod v1 — the recommended default |
| `LibGpiodV2Driver` | Modern Linux with libgpiod v2 |
| `RaspberryPi3Driver` | Raspberry Pi 3/4 — auto-selected, legacy direct memory access |
| `SysFsDriver` | Very old Linux kernels (pre-4.8) only — **deprecated** |

### Auto-Detection

When you use `new GpioController()`, the framework selects automatically:

- **Raspberry Pi 3/4:** `RaspberryPi3Driver`
- **Raspberry Pi 5:** `LibGpiodDriver` (with correct GPIO chip)
- **Other Linux boards:** Tries `LibGpiodDriver` → `LibGpiodV2Driver` → `SysFsDriver`

For an in-depth comparison, see [Choosing the Right Driver](../fundamentals/choosing-drivers.md).

### Manual Driver Selection

If auto-detection does not work for your board:

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Specify LibGpiodDriver with chip number (most boards use 0)
using GpioController controller = new(new LibGpiodDriver(gpioChip: 0));
```

### Installing libgpiod

Most Raspberry Pi OS images come with libgpiod pre-installed. If not:

```bash
sudo apt update
sudo apt install libgpiod2
```

For more detail on libgpiod versions and building from source, see [Using libgpiod to control GPIOs](../gpio-linux-libgpiod.md).

## Raspberry Pi 5 Note

Raspberry Pi 5 moved its GPIO to a different chip number. If you are explicitly constructing a driver, use chip **4** instead of 0:

```csharp
// Raspberry Pi 5 only
using GpioController controller = new(new LibGpiodDriver(gpioChip: 4));
```

The parameterless `new GpioController()` constructor handles this automatically on Pi 5, so you only need to worry about this when creating a driver manually.

To find the correct chip number on any board:

```bash
gpioinfo
```

## Troubleshooting

### "Permission denied" Error

```text
System.UnauthorizedAccessException: Access to GPIO is denied
```

**Solution:** Add your user to the `gpio` group, then **log out and back in**:

```bash
sudo usermod -aG gpio $USER
```

### "libgpiod not found" Error

```text
System.DllNotFoundException: Unable to load shared library 'libgpiod'
```

**Solution:** Install libgpiod:

```bash
sudo apt install libgpiod2
```

### Pin Already in Use

```text
System.InvalidOperationException: Pin 18 is already in use
```

**Solutions:**

1. Another process is using the pin — stop it or reboot
2. Pin not properly disposed in previous run — ensure you use `using` statements
3. Pin reserved by kernel (I2C, SPI, etc.) — use a different pin

### GPIO Operations Have No Effect / Wrong Pin

Make sure you are using **BCM/GPIO numbers**, not physical pin numbers. GPIO 18 is physical pin 12 — they are different numbers for the same pin. Consult [pinout.xyz](https://pinout.xyz/).

## Best Practices

1. **Use `using` statements** — Ensures proper disposal and cleanup of GPIO resources
2. **Use `InputPullUp` or `InputPullDown`** — Avoid floating inputs
3. **Check voltage levels** — Raspberry Pi uses 3.3 V; connecting 5 V **will damage the pin**
4. **Use resistors with LEDs** — 220 Ω is a safe default
5. **Document pin usage** — Keep track of which pins are used for what

## Related Documentation

- [GPIO Basics](../fundamentals/gpio-basics.md) — Detailed explanation of digital I/O, pull resistors, voltage levels, and current limits
- [Choosing the Right Driver](../fundamentals/choosing-drivers.md) — In-depth comparison of LibGpiodDriver, LibGpiodV2Driver, and SysFsDriver
- [Using libgpiod to control GPIOs](../gpio-linux-libgpiod.md) — libgpiod library versions and installation from source

## External Resources

- [GPIO Wikipedia](https://en.wikipedia.org/wiki/General-purpose_input/output)
- [Raspberry Pi GPIO Pinout](https://pinout.xyz/)
- [libgpiod Documentation](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/)
- [Linux GPIO Documentation](https://www.kernel.org/doc/html/latest/driver-api/gpio/index.html)
