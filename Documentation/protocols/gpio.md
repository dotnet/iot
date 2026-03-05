# GPIO (General-Purpose Input/Output)

General-Purpose Input/Output (GPIO) pins are programmable pins on your single-board computer that can be used to interface with external devices. This guide covers the basics of GPIO in .NET IoT and how to use the GpioController to control GPIO pins.

## What is GPIO?

GPIO pins allow you to:
- **Output digital signals** - Turn devices on/off (LEDs, relays, motors)
- **Read digital signals** - Detect button presses, sensor states
- **Generate PWM signals** - Control brightness, speed (requires specific PWM-capable pins)
- **Communicate via protocols** - Some GPIO pins support I2C, SPI, UART protocols

GPIO pins operate with digital logic levels (HIGH/LOW):
- **HIGH** = 3.3V on Raspberry Pi (varies by platform)
- **LOW** = 0V (Ground)

## Pin Numbering

.NET IoT uses **BCM (Broadcom) GPIO numbering**, not physical pin numbers:

```csharp
// GPIO 18 refers to BCM GPIO18 (which is physical pin 12 on Raspberry Pi)
controller.OpenPin(18, PinMode.Output);
```

**Important:** Always use GPIO/BCM numbers in your code. Use a pinout diagram for your specific board to map GPIO numbers to physical positions. For Raspberry Pi, see [pinout.xyz](https://pinout.xyz/).

## Basic Example

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

// Create GPIO controller
using GpioController controller = new();

// Open pin 18 as output
controller.OpenPin(18, PinMode.Output);

// Blink LED
for (int i = 0; i < 5; i++)
{
    controller.Write(18, PinValue.High);
    Thread.Sleep(1000);
    controller.Write(18, PinValue.Low);
    Thread.Sleep(1000);
}
```

## GpioController and GPIO Drivers

The `GpioController` class is the main entry point for GPIO operations in .NET IoT. It abstracts the underlying platform-specific GPIO implementation through **GPIO drivers**.

### Available GPIO Drivers

.NET IoT provides several GPIO driver implementations:

#### 1. LibGpiodDriver (Recommended for Linux)

Uses the modern `libgpiod` library to access GPIO through the Linux character device interface.

```csharp
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Explicitly use LibGpiodDriver
// Chip number: 0 for Pi 3/4, 4 for Pi 5
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(0));
```

**Advantages:**
- Modern, actively maintained
- Better security and resource management
- Supports multiple processes accessing GPIO simultaneously
- Required for Linux kernel 4.8+

**Requirements:**
- Install libgpiod library: `sudo apt install libgpiod2`
- User must be in `gpio` group: `sudo usermod -aG gpio $USER`

#### 2. RaspberryPi3Driver

A legacy driver specific to Raspberry Pi that uses direct memory access. This driver is now deprecated in favor of LibGpiodDriver.

```csharp
using System.Device.Gpio.Drivers;

// Legacy approach - not recommended
using GpioController controller = new(PinNumberingScheme.Logical, new RaspberryPi3Driver());
```

**Note:** This driver is kept for backward compatibility but should not be used in new projects. Use LibGpiodDriver instead.

#### 3. SysFsDriver (Deprecated)

Uses the older `/sys/class/gpio` interface, which is deprecated in modern Linux kernels.

```csharp
using System.Device.Gpio.Drivers;

// Legacy driver - only use if you must
using GpioController controller = new(PinNumberingScheme.Logical, new SysFsDriver());
```

**When to use:** Only for very old Linux kernels (pre-4.8) or legacy compatibility.

#### 4. Windows10Driver

For Windows 10 IoT Core (now discontinued).

### Auto-Detection

If you don't specify a driver, .NET IoT automatically selects the best available driver for your platform:

```csharp
// Automatically selects LibGpiodDriver on Linux if available
using GpioController controller = new();
```

**Recommendation:** For most applications, use the default auto-detection. Only specify a driver explicitly if you have specific requirements.

## Pin Modes

GPIO pins can be configured in different modes:

```csharp
// Output - drive pin HIGH or LOW
controller.OpenPin(18, PinMode.Output);

// Input - read pin state (floating, not recommended)
controller.OpenPin(17, PinMode.Input);

// Input with pull-up resistor (default HIGH, button press = LOW)
controller.OpenPin(17, PinMode.InputPullUp);

// Input with pull-down resistor (default LOW, button press = HIGH)
controller.OpenPin(17, PinMode.InputPullDown);
```

**Best practice:** Always use `InputPullUp` or `InputPullDown` for input pins to avoid floating states.

## Reading and Writing

```csharp
// Write to output pin
controller.Write(18, PinValue.High);
controller.Write(18, PinValue.Low);

// Read from input pin
PinValue value = controller.Read(17);
if (value == PinValue.High)
{
    Console.WriteLine("Button pressed");
}
```

## Interrupt-Driven Input (Events)

Instead of continuously polling, use events for efficient input handling:

```csharp
// Register callback for pin value changes
controller.RegisterCallbackForPinValueChangedEvent(
    17,
    PinEventTypes.Falling,  // Trigger on HIGH → LOW
    (sender, args) =>
    {
        Console.WriteLine($"Button pressed at {args.ChangeTime}");
    });

// Keep program running
Console.WriteLine("Press button. Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);
```

**Event types:**
- `PinEventTypes.Rising` - LOW → HIGH transition
- `PinEventTypes.Falling` - HIGH → LOW transition  
- `PinEventTypes.Rising | PinEventTypes.Falling` - Both transitions

## Advanced: LibGpiodDriver Configuration

### Specifying GPIO Chip Number

Different platforms use different GPIO chip numbers:

```csharp
// Raspberry Pi 3/4
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(0));

// Raspberry Pi 5
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(4));
```

**Find your chip number:**
```bash
gpioinfo
```

### LibGpiodDriver Versions

libgpiod library has multiple versions. .NET IoT supports both v1 and v2:

| LibGpiodDriverVersion | Libgpiod Library Version |
|-----------------------|--------------------------|
| V1                    | 0.x - 1.x                |
| V2                    | 2.x                      |

**Auto-detection (recommended):**
```csharp
// Automatically detects and uses correct version
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(0));
```

**Manual version selection:**
```csharp
using System.Device.Gpio.Drivers;

// Force V1 driver
var driver = new LibGpiodDriver(0, LibGpiodDriverVersion.V1);
using GpioController controller = new(PinNumberingScheme.Logical, driver);

// Force V2 driver
var driver2 = new LibGpiodDriver(0, LibGpiodDriverVersion.V2);
```

**Or using environment variable:**
```bash
export DOTNET_IOT_LIBGPIOD_DRIVER_VERSION=V1
dotnet run
```

### Installing libgpiod

**From package manager:**
```bash
sudo apt update
sudo apt install libgpiod2
```

**From source (for latest version):**
```bash
# Install dependencies
sudo apt update && sudo apt install -y autogen autoconf autoconf-archive libtool libtool-bin pkg-config build-essential

# Download and extract (check https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/refs/ for latest)
wget https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/snapshot/libgpiod-2.1.tar.gz
tar -xzf libgpiod-2.1.tar.gz
cd libgpiod-2.1/

# Compile and install
./autogen.sh --enable-tools=yes
make
sudo make install
sudo ldconfig
```

## Troubleshooting

### "Permission denied" Error

```
System.UnauthorizedAccessException: Access to GPIO is denied
```

**Solution:** Add user to gpio group:
```bash
sudo usermod -aG gpio $USER
# Log out and log back in
```

### "Cannot access GPIO chip" / Chip Number Issues

**On Raspberry Pi 5:**
Raspberry Pi 5 uses chip number **4** instead of 0:
```csharp
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(4));
```

**Find correct chip:**
```bash
gpioinfo
```

### "libgpiod not found" Error

```
System.DllNotFoundException: Unable to load shared library 'libgpiod'
```

**Solution:** Install libgpiod:
```bash
sudo apt install libgpiod2
```

### Pin Already in Use

```
System.InvalidOperationException: Pin 18 is already in use
```

**Solutions:**
1. Another process is using the pin - close it or reboot
2. Pin not properly disposed in previous run - ensure you use `using` statements
3. Pin reserved by kernel (I2C, SPI, etc.) - use different pin

### Wrong Pin Numbers

If GPIO operations don't work as expected, verify you're using **BCM/GPIO numbers**, not physical pin numbers. Consult a pinout diagram for your board.

## Best Practices

1. **Use `using` statements** - Ensures proper disposal and cleanup
2. **Use InputPullUp/InputPullDown** - Avoid floating inputs
3. **Handle debouncing** - Physical buttons need software or hardware debouncing
4. **Check voltage levels** - Raspberry Pi uses 3.3V, some devices use 5V
5. **Limit current** - Use resistors with LEDs, transistors for high-current loads
6. **Document pin usage** - Keep track of which pins are used for what

## Related Documentation

- [GPIO Basics](../fundamentals/gpio-basics.md) - Detailed explanation of digital I/O, pull resistors, voltage levels
- [Choosing Drivers](../fundamentals/choosing-drivers.md) - In-depth comparison of LibGpiodDriver vs SysFsDriver
- [Signal Debouncing](../fundamentals/debouncing.md) - How to handle noisy button inputs
- [Raspberry Pi 5 Guide](../platforms/raspberry-pi-5.md) - Platform-specific information for Raspberry Pi 5
- [Troubleshooting Guide](../troubleshooting.md) - Common GPIO issues and solutions

## External Resources

- [GPIO Wikipedia](https://en.wikipedia.org/wiki/General-purpose_input/output)
- [Raspberry Pi GPIO Pinout](https://pinout.xyz/)
- [libgpiod Documentation](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/)
- [Linux GPIO Documentation](https://www.kernel.org/doc/html/latest/driver-api/gpio/index.html)
