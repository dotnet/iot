# Getting Started with .NET IoT

Welcome to .NET IoT! This guide will help you get started with building IoT applications using .NET on single-board computers like Raspberry Pi.

## Quick Start: Blink an LED in 5 Minutes

### Prerequisites

- A Raspberry Pi (3, 4, or 5) with Raspberry Pi OS installed
- .NET 8.0 SDK or later installed ([installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian))
- An LED and a 220Ω resistor
- A breadboard and jumper wires

### Step 1: Create a New Project

```bash
dotnet new console -n LedBlink
cd LedBlink
dotnet add package System.Device.Gpio
```

### Step 2: Write the Code

Replace the contents of `Program.cs` with:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

int pin = 18;
using GpioController controller = new();
controller.OpenPin(pin, PinMode.Output);

Console.WriteLine("Blinking LED. Press Ctrl+C to exit.");

while (true)
{
    controller.Write(pin, PinValue.High);
    Thread.Sleep(1000);
    controller.Write(pin, PinValue.Low);
    Thread.Sleep(1000);
}
```

### Step 3: Wire the LED

Connect the LED to your Raspberry Pi:

1. Connect the **positive leg (longer leg)** of the LED to **GPIO 18** (physical pin 12)
2. Connect the **negative leg** through a **220Ω resistor** to **Ground** (physical pin 6)

### Step 4: Run the Application

```bash
dotnet run
```

You should see the LED blinking on and off every second!

## What's Next?

Now that you have your first IoT application running, explore these topics to deepen your knowledge:

### Learn the Fundamentals

- [GPIO Basics](fundamentals/gpio-basics.md) - Understanding digital input/output, pull-up/pull-down resistors
- [Understanding Protocols](fundamentals/understanding-protocols.md) - When to use I2C, SPI, or UART
- [Choosing the Right Driver](fundamentals/choosing-drivers.md) - libgpiod vs sysfs
- [Signal Debouncing](fundamentals/debouncing.md) - Handling noisy button inputs

### Set Up Communication Protocols

- [I2C Setup and Usage](protocols/i2c.md) - Connect I2C sensors and displays
- [SPI Setup and Usage](protocols/spi.md) - High-speed communication with SPI devices
- [PWM Setup and Usage](protocols/pwm.md) - Control LED brightness and motors
- [UART/Serial Setup](protocols/uart.md) - RS232/RS485 communication

### Platform-Specific Guides

- [Raspberry Pi 5 Guide](platforms/raspberry-pi-5.md) - Specific instructions for Raspberry Pi 5
- [Raspberry Pi 4 Guide](platforms/raspberry-pi-4.md) - Specific instructions for Raspberry Pi 4

### Deploy Your Application

- [Running in Docker Containers](deployment/containers.md) - Containerize your IoT apps
- [Auto-start on Boot](deployment/systemd-services.md) - Run apps automatically with systemd
- [Cross-compilation](deployment/cross-compilation.md) - Build on your PC, run on Raspberry Pi

### Use Device Bindings

Explore the [130+ device bindings](../src/devices/README.md) for sensors, displays, and other peripherals:

- Temperature sensors (DHT22, BME280, DS18B20)
- Displays (SSD1306 OLED, LCD1602, MAX7219)
- Motion sensors (HC-SR04 ultrasonic, PIR)
- And many more!

## Common Pitfalls and Solutions

### Permission Denied Errors

If you see permission errors when accessing GPIO:

```bash
sudo usermod -aG gpio $USER
```

Then log out and log back in.

### Device Not Found Errors

If you see "Device not found" errors for I2C, SPI, or PWM:

- Check that the interface is enabled in `raspi-config`
- Verify your wiring matches the pin numbers in your code
- Use diagnostic tools (`i2cdetect`, `gpioinfo`) to verify hardware

### Wrong Pin Numbers

Be aware of different pin numbering schemes:

- **GPIO/BCM numbering** (used by .NET IoT): GPIO 18 is the GPIO number
- **Physical numbering**: Pin 12 is the physical position on the header
- Always use GPIO/BCM numbers in your code

## Need Help?

- [Troubleshooting Guide](troubleshooting.md) - Common issues and solutions
- [Glossary](glossary.md) - Terms and concepts explained
- [GitHub Issues](https://github.com/dotnet/iot/issues) - Report bugs or ask questions
- [.NET IoT Documentation](https://docs.microsoft.com/dotnet/iot/) - Official documentation

## Hardware Safety Tips

- Always use current-limiting resistors with LEDs (220Ω is typical)
- Never connect 5V devices directly to 3.3V GPIO pins
- Double-check your wiring before powering on
- Use a multimeter to verify voltages and connections
- Power off the device before changing wiring

Happy building!
