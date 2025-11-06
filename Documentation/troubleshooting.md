# Troubleshooting Common Issues

This guide helps you diagnose and fix common problems when developing .NET IoT applications.

## Quick Diagnostic Commands

```bash
# Check .NET version
dotnet --version

# List GPIO chips
gpioinfo

# Scan I2C bus
i2cdetect -y 1

# List SPI devices
ls /dev/spidev*

# List serial ports
ls /dev/tty*

# Check system logs
sudo journalctl -xe

# Monitor system messages
dmesg | tail -20
```

## GPIO Issues

### "Cannot access GPIO" / Permission Denied

**Error:**

```
System.UnauthorizedAccessException: Access to GPIO is denied
```

**Causes & Solutions:**

1. **User not in gpio group:**

```bash
sudo usermod -aG gpio $USER
# Log out and log back in
groups  # Verify 'gpio' is in the list
```

2. **Wrong chip number (Raspberry Pi 5):**

```csharp
// Raspberry Pi 5 uses chip 4, not 0
using GpioController controller = new(PinNumberingScheme.Logical, new LibGpiodDriver(4));

// Or let it auto-detect
using GpioController controller = new();
```

3. **libgpiod not installed:**

```bash
sudo apt install libgpiod2
```

### "Pin is already in use"

**Error:**

```
System.InvalidOperationException: Pin 18 is already in use
```

**Causes & Solutions:**

1. **Another process is using the pin:**

```bash
# Find which process is using GPIO
sudo lsof | grep gpiochip
# or
ps aux | grep dotnet
```

Kill the process or close it properly.

2. **Pin not closed in previous run:**

Make sure to dispose GpioController:

```csharp
using GpioController controller = new();
// Automatically disposed when out of scope
```

Or explicitly:

```csharp
controller.ClosePin(18);
controller.Dispose();
```

3. **Pin used by kernel driver:**

Some pins are reserved by kernel (I2C, SPI, UART). Use different pin.

### GPIO Reads Always HIGH or Always LOW

**Possible Causes:**

1. **Missing pull-up/pull-down resistor:**

```csharp
// Add pull-up or pull-down
controller.OpenPin(17, PinMode.InputPullUp);
// or
controller.OpenPin(17, PinMode.InputPullDown);
```

2. **Wrong wiring:**

- Check connections with multimeter
- Verify ground is connected
- Test with known-good LED circuit

3. **Pin damaged:**

Test with different pin.

### Button Presses Register Multiple Times

**Cause:** Button bouncing (mechanical noise).

**Solution:** Implement debouncing.

```csharp
const int debounceMs = 50;
DateTime lastPress = DateTime.MinValue;

controller.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling,
    (sender, args) =>
    {
        if ((DateTime.UtcNow - lastPress).TotalMilliseconds >= debounceMs)
        {
            lastPress = DateTime.UtcNow;
            // Handle button press
        }
    });
```

See [Debouncing Guide](fundamentals/debouncing.md) for more solutions.

## I2C Issues

### "Device not found" / "Error 2"

**Error:**

```
System.IO.IOException: Error 2. Can not open I2C device file '/dev/i2c-1'
```

**Causes & Solutions:**

1. **I2C not enabled:**

```bash
sudo raspi-config
# Interface Options → I2C → Enable
sudo reboot
```

Or manually:

```bash
sudo nano /boot/firmware/config.txt
# Add: dtparam=i2c_arm=on
sudo reboot
```

2. **Wrong bus number:**

Try bus 0 instead of 1:

```csharp
I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(0, 0x76));
```

List available buses:

```bash
ls /dev/i2c-*
```

3. **User not in i2c group:**

```bash
sudo usermod -aG i2c $USER
# Log out and log back in
```

### "Error 121" / Remote I/O Error

**Error:**

```
System.IO.IOException: Error 121 performing I2C data transfer
```

**Meaning:** Device not responding at specified address.

**Causes & Solutions:**

1. **Wrong I2C address:**

Scan for devices:

```bash
i2cdetect -y 1
```

Shows grid with addresses. Update your code with correct address:

```csharp
// If device shows at 0x77 instead of 0x76
I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 0x77));
```

2. **Wiring issues:**

- Check SDA and SCL connections
- Verify ground is connected
- Check for loose wires

3. **Missing pull-up resistors:**

I2C requires pull-up resistors on SDA and SCL (typically 4.7kΩ). Most sensor modules have them built-in, but check:

```
     3.3V
      │
     ┌┴┐
     │ │ 4.7kΩ (both SDA and SCL need this)
     └┬┘
```

4. **Device not powered:**

- Check device has power (3.3V or 5V as required)
- Verify with multimeter

5. **Device broken or incompatible:**

Test with known-good device.

### I2C Speed Issues

Some devices don't support fast mode (400kHz).

**Solution:** Reduce I2C speed in `/boot/firmware/config.txt`:

```text
dtparam=i2c_arm_baudrate=100000
```

Reboot after change.

## SPI Issues

### "Cannot open SPI device"

**Error:**

```
System.IO.IOException: Error 2. Can not open SPI device file '/dev/spidev0.0'
```

**Causes & Solutions:**

1. **SPI not enabled:**

```bash
sudo raspi-config
# Interface Options → SPI → Enable
sudo reboot
```

2. **User not in spi group:**

```bash
sudo usermod -aG spi $USER
# Log out and log back in
```

3. **Wrong device file:**

Check available SPI devices:

```bash
ls /dev/spidev*
```

Use correct bus and chip select:

```csharp
// Bus 0, CS 0
SpiDevice device = SpiDevice.Create(new SpiConnectionSettings(0, 0));
```

### SPI Communication Not Working

**Symptoms:** No data or wrong data received.

**Causes & Solutions:**

1. **Wrong SPI mode:**

Check device datasheet for correct mode (0-3):

```csharp
SpiDevice device = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    Mode = SpiMode.Mode0,  // Try different modes
    ClockFrequency = 1_000_000
});
```

2. **Clock frequency too high:**

Reduce clock speed:

```csharp
ClockFrequency = 500_000  // 500kHz instead of 1MHz
```

3. **Wiring issues:**

Verify connections:

- MOSI (GPIO 10) → MOSI on device
- MISO (GPIO 9) → MISO on device
- SCK (GPIO 11) → SCK on device
- CS (GPIO 8) → CS on device
- Ground → Ground

4. **Chip Select polarity:**

Some devices use active-high CS. Check datasheet.

## UART/Serial Issues

### "Port does not exist"

**Error:**

```
System.IO.IOException: The port '/dev/ttyS0' does not exist
```

**Causes & Solutions:**

1. **UART not enabled:**

```bash
sudo raspi-config
# Interface Options → Serial Port
# Login shell: No
# Hardware: Yes
sudo reboot
```

2. **Wrong device name:**

Try different port names:

```csharp
// Try these in order
"/dev/serial0"  // Recommended (symbolic link)
"/dev/ttyS0"    // Mini UART
"/dev/ttyAMA0"  // Full UART
"/dev/ttyUSB0"  // USB-Serial adapter
```

List available ports:

```bash
ls /dev/tty*
```

### No Data Received / Garbage Characters

**Causes & Solutions:**

1. **Wrong baud rate:**

Both sides must match exactly:

```csharp
SerialPort port = new SerialPort("/dev/serial0", 9600);  // Try device's baud rate
```

Common rates: 9600, 19200, 38400, 57600, 115200.

2. **TX/RX swapped:**

Verify wiring:

- Raspberry Pi TX (GPIO 14) → Device RX
- Raspberry Pi RX (GPIO 15) → Device TX

3. **Serial console still active:**

Disable console:

```bash
sudo systemctl disable serial-getty@ttyS0.service
sudo systemctl disable serial-getty@ttyAMA0.service
sudo reboot
```

4. **Wrong data format:**

Check device requires 8N1 (default) or different:

```csharp
SerialPort port = new SerialPort("/dev/serial0", 9600, 
    Parity.None,    // Try Parity.Even or Parity.Odd
    8,              // Try 7 data bits
    StopBits.One);
```

5. **Voltage level mismatch:**

Use level shifter for 5V devices (Raspberry Pi is 3.3V).

### Permission Denied

```bash
sudo usermod -aG dialout $USER
# Log out and log back in
```

## PWM Issues

### "Chip number is invalid"

**Error:**

```
System.ArgumentException: The chip number 0 is invalid or is not enabled
```

**Causes & Solutions:**

1. **PWM not enabled:**

Edit `/boot/firmware/config.txt`:

```bash
sudo nano /boot/firmware/config.txt
```

Add:

```text
dtoverlay=pwm,pin=18,func=2
```

Reboot:

```bash
sudo reboot
```

2. **User not in pwm group:**

```bash
sudo usermod -aG gpio $USER
# Log out and log back in
```

3. **Wrong pin:**

Use PWM-capable pins only: GPIO 12, 13, 18, 19.

### PWM Not Working / No Output

**Causes & Solutions:**

1. **Wrong chip/channel:**

```csharp
// For GPIO 18: chip 0, channel 0
PwmChannel pwm = PwmChannel.Create(0, 0, 400, 0.5);
```

Mapping:

- GPIO 12/18: chip 0, channel 0
- GPIO 13/19: chip 0, channel 1

2. **Frequency too high/low:**

Try different frequencies:

```csharp
PwmChannel pwm = PwmChannel.Create(0, 0, 1000, 0.5);  // 1kHz
```

3. **Start PWM:**

Don't forget to start:

```csharp
pwm.Start();
```

## Build and Runtime Issues

### ".NET SDK not found"

**Error:**

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application
```

**Solution:**

Install .NET SDK:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0

# Add to PATH
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

### "Package not found"

**Error:**

```
error NU1101: Unable to find package System.Device.Gpio
```

**Solution:**

1. **Restore packages:**

```bash
dotnet restore
```

2. **Check internet connection:**

```bash
ping nuget.org
```

3. **Clear NuGet cache:**

```bash
dotnet nuget locals all --clear
dotnet restore
```

### Application Crashes on GPIO Access

**Symptoms:** Segmentation fault, access violation.

**Causes & Solutions:**

1. **libgpiod version mismatch:**

```bash
# Check installed version
apt show libgpiod2

# Force specific driver version
export DOTNET_IOT_LIBGPIOD_DRIVER_VERSION=V2
dotnet run
```

2. **Corrupted installation:**

Reinstall libgpiod:

```bash
sudo apt remove libgpiod2
sudo apt autoremove
sudo apt update
sudo apt install libgpiod2
```

### "Unable to load shared library 'libgpiod'"

**Error:**

```
System.DllNotFoundException: Unable to load shared library 'libgpiod'
```

**Solution:**

```bash
sudo apt install libgpiod2
```

For manual installation:

```bash
sudo ldconfig  # Refresh library cache
```

## Hardware Issues

### LED Doesn't Light Up

**Causes & Solutions:**

1. **LED backwards:**

LEDs are polarized. Longer leg is positive (anode).

2. **Missing resistor:**

Always use current-limiting resistor (220Ω typical).

3. **Pin not set to output:**

```csharp
controller.OpenPin(18, PinMode.Output);
controller.Write(18, PinValue.High);
```

4. **Voltage too low:**

LED forward voltage must be less than 3.3V. Blue/white LEDs may not work (need ~3.4V).

### Sensor Not Responding

**Diagnostic Steps:**

1. **Check power:**

Measure voltage with multimeter (should be 3.3V or 5V as required).

2. **Verify wiring:**

Double-check all connections match datasheet.

3. **Scan bus:**

```bash
# I2C
i2cdetect -y 1

# Check device files exist
ls /dev/i2c-* /dev/spidev*
```

4. **Test with simple code:**

Try reading device ID register first (usually 0xD0 or 0x00).

5. **Check datasheets:**

Verify timing requirements, startup delays, initialization sequence.

### Random Behavior / Intermittent Issues

**Causes & Solutions:**

1. **Power supply insufficient:**

Use quality 5V/3A+ power supply. Check for undervoltage warning:

```bash
vcgencmd get_throttled
```

2. **Loose connections:**

Check all wires are firmly connected.

3. **EMI/RFI interference:**

- Use shielded cables
- Keep wires short
- Add decoupling capacitors (0.1µF) near chips

4. **Overheating:**

Check temperature:

```bash
vcgencmd measure_temp
```

Add cooling if > 70°C.

## Getting Help

### Before Asking for Help

1. ✅ Read error message carefully
2. ✅ Check this troubleshooting guide
3. ✅ Verify hardware with multimeter
4. ✅ Test with known-good example code
5. ✅ Search GitHub issues: [dotnet/iot issues](https://github.com/dotnet/iot/issues)

### When Asking for Help

Include:

- **Hardware:** Raspberry Pi model, sensor/device model
- **OS:** Raspberry Pi OS version (`cat /etc/os-release`)
- **Error message:** Complete error with stack trace
- **.NET version:** `dotnet --version`
- **Code:** Minimal reproducible example
- **Wiring:** Describe connections or provide diagram
- **What you tried:** Steps already attempted

### Resources

- [.NET IoT GitHub Discussions](https://github.com/dotnet/iot/discussions)
- [.NET IoT GitHub Issues](https://github.com/dotnet/iot/issues)
- [Raspberry Pi Forums](https://forums.raspberrypi.com/)
- [Stack Overflow - .NET IoT tag](https://stackoverflow.com/questions/tagged/.net-iot)

## Preventive Measures

1. ✅ **Double-check wiring** before powering on
2. ✅ **Use adequate power supply** (5V/3A minimum)
3. ✅ **Measure voltages** with multimeter when in doubt
4. ✅ **Dispose resources properly** - Use `using` statements
5. ✅ **Add error handling** - Catch exceptions, log errors
6. ✅ **Start simple** - Test hardware with basic code first
7. ✅ **Keep backups** - SD card images of working systems
8. ✅ **Use version control** - Git for your code

## See Also

- [GPIO Basics](fundamentals/gpio-basics.md)
- [Understanding Protocols](fundamentals/understanding-protocols.md)
- [Reading Datasheets](fundamentals/reading-datasheets.md)
- [Glossary](glossary.md)
