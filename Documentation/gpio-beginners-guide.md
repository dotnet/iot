# GPIO Guide for Beginners

General-Purpose Input/Output (GPIO) pins are programmable electrical pins on single-board computers like the Raspberry Pi. They let your .NET code interact with physical hardware — turning LEDs on and off, reading button presses, and communicating with sensors.

This guide will walk you through everything you need to know to get started with GPIO using the .NET IoT library.

## What You Need

- A **Raspberry Pi** (3, 4, or 5) running Raspberry Pi OS
- **.NET 8.0 SDK** or later installed ([installation guide](https://learn.microsoft.com/dotnet/core/install/linux-debian))
- A **breadboard**, a few **jumper wires**, an **LED**, a **220 Ω resistor**, and a **push button**

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

const int ledPin = 18; // GPIO 18 = physical pin 12 on Raspberry Pi

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

1. **GPIO 18** (physical pin 12) → long leg (anode, +) of LED
2. Short leg (cathode, −) of LED → one end of **220 Ω resistor**
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

## Key Concept: Pin Numbering

There are two ways to refer to pins, which can be confusing at first:

| Scheme | Example | Description |
| --- | --- | --- |
| **GPIO / BCM** | `18` | The number used by the Broadcom chip — **this is what .NET IoT uses** |
| **Physical / Board** | `12` | The physical position on the 40-pin header |

GPIO 18 and physical pin 12 are the **same pin** — just identified differently. Your code always uses **GPIO (BCM) numbers**.

Use the interactive diagram at [pinout.xyz](https://pinout.xyz/) to look up the mapping for your board.

## Digital Output: Controlling Devices

Setting a pin to **High** sends 3.3 V to the pin. Setting it to **Low** sends 0 V. That is all digital output does: voltage on or voltage off.

```csharp
using GpioController controller = new();

controller.OpenPin(18, PinMode.Output);

controller.Write(18, PinValue.High); // 3.3 V — turns the LED on
controller.Write(18, PinValue.Low);  // 0 V   — turns the LED off
```

You can use this to drive LEDs, buzzers, relays, and any component that responds to a high/low signal.

### Current Limits

The Raspberry Pi's GPIO pins can supply a maximum of about **16 mA per pin**. An LED with a 220 Ω resistor draws about 6 mA — well within this limit.

For higher-power devices (motors, relays, strips of LEDs) you need to use a transistor or MOSFET as a switch, powered from an external supply. The GPIO pin controls the transistor's gate, not the device directly.

## Digital Input: Reading Buttons and Sensors

To read whether a pin is receiving a high or low signal, open it in an input mode:

```csharp
using GpioController controller = new();

controller.OpenPin(17, PinMode.InputPullUp);

PinValue value = controller.Read(17);

if (value == PinValue.Low)
{
    Console.WriteLine("Button is pressed!");
}
```

### Why InputPullUp Instead of Input?

When a button is **not** pressed, the pin is not connected to anything. In plain `Input` mode, this leaves the pin "floating" — its voltage drifts randomly, giving unreliable readings.

Pull-up and pull-down resistors solve this by defining a default voltage:

| Mode | Default state (not pressed) | Pressed state | Wiring |
| --- | --- | --- | --- |
| `InputPullUp` | High (3.3 V) | Low (0 V) | Button connects pin to Ground |
| `InputPullDown` | Low (0 V) | High (3.3 V) | Button connects pin to 3.3 V |

`InputPullUp` is the most common choice for buttons. The Raspberry Pi has built-in pull-up resistors that are activated when you use this mode — no external resistor required.

### Button Wiring (with InputPullUp)

```text
GPIO 17 ──► one leg of button
GND     ──► other leg of button
```

When the button is pressed, GPIO 17 gets connected to Ground (Low). When it is released, the internal pull-up resistor pulls it back to High.

## Reacting to Changes with Events

Instead of checking the pin over and over in a loop (polling), you can register a callback that fires when the pin's value changes. This is more efficient and more responsive:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

const int buttonPin = 17;

using GpioController controller = new();

controller.OpenPin(buttonPin, PinMode.InputPullUp);

controller.RegisterCallbackForPinValueChangedEvent(
    buttonPin,
    PinEventTypes.Falling, // trigger on High → Low (button press)
    (sender, args) =>
    {
        Console.WriteLine($"Button pressed on pin {args.PinNumber}");
    });

Console.WriteLine("Press the button. Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);
```

**Event types:**

- `PinEventTypes.Rising` — Low → High transition
- `PinEventTypes.Falling` — High → Low transition
- `PinEventTypes.Rising | PinEventTypes.Falling` — both directions

> **Note:** Physical buttons "bounce" — a single press can generate several rapid transitions. For reliable button handling, see the debouncing techniques described in [PR #2438's debouncing guide](https://github.com/dotnet/iot/pull/2438).

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
    // Button pressed = Low (because of InputPullUp)
    PinValue buttonState = controller.Read(buttonPin);
    controller.Write(ledPin, buttonState == PinValue.Low ? PinValue.High : PinValue.Low);
    Thread.Sleep(50);
}
```

## GPIO Drivers: How .NET Talks to Hardware

Underneath, `GpioController` uses a **driver** to communicate with the operating system's GPIO interface. You usually do not need to think about this — the parameterless constructor automatically picks the best driver:

```csharp
// Recommended — auto-detects the best driver for your board
using GpioController controller = new();
```

The auto-detection works like this:

| Board | Driver selected |
| --- | --- |
| Raspberry Pi 3 / 4 | `RaspberryPi3Driver` |
| Raspberry Pi 5 | `LibGpiodDriver` (with correct chip) |
| Other Linux boards | Tries `LibGpiodDriver` → `LibGpiodV2Driver` → `SysFsDriver` |

### When to Choose a Driver Manually

You only need to specify a driver explicitly if:

- Auto-detection picks the wrong driver for your board
- You need to target a specific GPIO chip number
- You are troubleshooting GPIO access issues

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Specify LibGpiodDriver with chip number (most boards use 0)
using GpioController controller = new(new LibGpiodDriver(gpioChip: 0));
```

### Available Drivers

| Driver | When to use |
| --- | --- |
| `LibGpiodDriver` | Modern Linux with libgpiod v1 — the recommended default |
| `LibGpiodV2Driver` | Modern Linux with libgpiod v2 |
| `RaspberryPi3Driver` | Raspberry Pi 3/4 — auto-selected, legacy direct memory access |
| `SysFsDriver` | Very old Linux kernels (pre-4.8) only — **deprecated** |

### Installing libgpiod

Most Raspberry Pi OS images come with libgpiod pre-installed. If not:

```bash
sudo apt update
sudo apt install libgpiod2
```

For more detail on libgpiod versions and building from source, see [Using libgpiod to control GPIOs](gpio-linux-libgpiod.md).

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

### "Permission denied" when accessing GPIO

Add your user to the `gpio` group, then **log out and back in**:

```bash
sudo usermod -aG gpio $USER
```

### "libgpiod not found" / DllNotFoundException

Install the library:

```bash
sudo apt install libgpiod2
```

### GPIO operations have no effect / wrong pin

Make sure you are using **GPIO (BCM) numbers**, not physical pin numbers. GPIO 18 is physical pin 12 — they are different numbers for the same pin. Consult [pinout.xyz](https://pinout.xyz/).

### "Pin is already in use"

This can happen if:

1. Another process is using the pin — stop it or reboot
2. A previous run did not dispose the controller properly — always use `using` statements
3. The pin is reserved by a kernel driver (I2C, SPI, UART) — choose a different pin

## Best Practices

1. **Always use `using` statements** — this ensures pins are released when your program exits
2. **Use `InputPullUp` or `InputPullDown`** — never leave input pins floating
3. **Respect voltage levels** — Raspberry Pi GPIO operates at 3.3 V; connecting 5 V **will damage the pin**
4. **Use resistors with LEDs** — calculate the value or use 220 Ω as a safe default
5. **Check pin assignments** — some GPIO pins are shared with I2C, SPI, or UART; using them for general GPIO disables those interfaces

## Voltage Safety

Raspberry Pi GPIO pins operate at **3.3 V**. Connecting a 5 V signal directly to a GPIO input **will permanently damage the pin**. If you need to interface with 5 V devices, use a level shifter or a voltage divider.

## Next Steps

- Browse the [130+ device bindings](../src/devices/README.md) for sensors, displays, motors, and more
- Read the [libgpiod guide](gpio-linux-libgpiod.md) for advanced driver configuration
- Check the [Raspberry Pi I2C setup](raspi-i2c.md), [SPI setup](raspi-spi.md), and [PWM setup](raspi-pwm.md) for other communication protocols
- Visit [pinout.xyz](https://pinout.xyz/) for an interactive Raspberry Pi pin reference
