---
name: hardware-abstraction-check
description: Guide for ensuring device bindings maintain proper hardware abstraction and cross-platform compatibility
license: MIT
---

## Purpose

This skill helps Copilot agents verify that device bindings use proper hardware abstractions (System.Device.*) instead of platform-specific code, ensuring compatibility across Linux, Windows, and embedded systems.

## When to Use This Skill

- Reviewing new device binding code
- Investigating cross-platform compatibility issues
- Code contains platform-specific APIs or P/Invoke
- Ensuring code works on Raspberry Pi, Windows IoT, and other platforms
- Validating changes don't introduce OS dependencies

## Instructions

### 1. Core Principle

**Device bindings must use System.Device.* abstractions exclusively.**

The repository provides cross-platform abstractions:

- `System.Device.Gpio` for GPIO pins
- `System.Device.I2c` for I²C communication
- `System.Device.Spi` for SPI communication  
- `System.Device.Pwm` for PWM signals

These abstractions handle platform differences internally.

### 2. Anti-Patterns to Detect

#### ❌ Direct File System Access

```csharp
// WRONG - hardcoded Linux GPIO paths
const string GpioPath = "/sys/class/gpio";
File.WriteAllText("/sys/class/gpio/export", "17");

// WRONG - platform-specific device paths
var i2cDevice = new I2cUnixDevice("/dev/i2c-1", 0x48);
```

✅ **CORRECT - Use abstractions:**

```csharp
using System.Device.Gpio;
using System.Device.I2c;

using GpioController controller = new();
controller.OpenPin(17, PinMode.Output);

I2cConnectionSettings settings = new(busId: 1, deviceAddress: 0x48);
using I2cDevice device = I2cDevice.Create(settings);
```

#### ❌ P/Invoke and DllImport

```csharp
// WRONG - platform-specific P/Invoke
[DllImport("libc.so.6")]
private static extern int ioctl(int fd, uint request, int value);

// WRONG - Windows-specific
[DllImport("kernel32.dll")]
private static extern bool DeviceIoControl(...);
```

✅ **CORRECT - Device abstractions handle this internally**

#### ❌ OS Detection and Branching

```csharp
// WRONG - OS-specific branching
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Linux GPIO implementation
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows GPIO implementation
}

// WRONG - Environment checks
if (Environment.OSVersion.Platform == PlatformID.Unix)
{
    // Unix-specific code
}
```

✅ **CORRECT - Abstractions handle platform differences:**

```csharp
// Works on all platforms
using GpioController controller = new();
controller.OpenPin(17, PinMode.Output);
```

#### ❌ Process Execution for Hardware Access

```csharp
// WRONG - shelling out to OS utilities
Process.Start("gpio", "mode 17 out");
Process.Start("i2cset", "-y 1 0x48 0x00 0xFF");
```

✅ **CORRECT - Use device APIs**

### 3. Automated Detection Commands

Run these checks on device binding code:

```bash
# Check for P/Invoke usage
grep -r "DllImport\|P/Invoke" src/devices/<DeviceName>/

# Check for OS detection
grep -r "Environment.OSVersion\|RuntimeInformation\|OSPlatform" src/devices/<DeviceName>/

# Check for hardcoded paths
grep -r "/sys/\|/dev/\|/proc/" src/devices/<DeviceName>/

# Check for process execution
grep -r "Process.Start\|ProcessStartInfo" src/devices/<DeviceName>/

# All of these should return minimal or no results
```

### 4. Acceptable Platform-Specific Code

Some cases may require platform-specific code:

#### When Platform-Specific Code is Acceptable

1. **Feature is inherently platform-specific** (e.g., Windows-only hardware)
2. **Properly guarded with clear error messages**
3. **Documented as platform-specific in XML comments**
4. **Fails gracefully on unsupported platforms**

✅ **Example of acceptable platform-specific code:**

```csharp
/// <summary>
/// Enables hardware feature X. Only supported on Linux.
/// </summary>
/// <exception cref="PlatformNotSupportedException">
/// Thrown on non-Linux platforms
/// </exception>
public void EnableAdvancedFeature()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        throw new PlatformNotSupportedException(
            "Advanced feature X requires Linux kernel 5.0+");
    }
    
    // Platform-specific implementation
}
```

### 5. Using Device Abstractions Correctly

#### GPIO Controller Usage

✅ **DO:**

```csharp
public class LedDevice : IDisposable
{
    private readonly GpioController _controller;
    private readonly int _pin;
    private readonly bool _shouldDispose;

    public LedDevice(int pin, GpioController? controller = null, 
                     bool shouldDispose = true)
    {
        _pin = pin;
        _shouldDispose = shouldDispose || controller is null;
        _controller = controller ?? new GpioController();
        _controller.OpenPin(_pin, PinMode.Output);
    }

    public void Dispose()
    {
        if (_shouldDispose)
        {
            _controller?.Dispose();
        }
    }
}
```

#### I2C Device Usage

✅ **DO:**

```csharp
public class I2cSensor : IDisposable
{
    private I2cDevice _i2cDevice;
    
    public I2cSensor(I2cDevice i2cDevice)
    {
        _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
    }

    public byte ReadRegister(byte register)
    {
        Span<byte> writeBuffer = stackalloc byte[] { register };
        Span<byte> readBuffer = stackalloc byte[1];
        _i2cDevice.WriteRead(writeBuffer, readBuffer);
        return readBuffer[0];
    }

    public void Dispose()
    {
        _i2cDevice?.Dispose();
        _i2cDevice = null;
    }
}
```

#### SPI Device Usage

✅ **DO:**

```csharp
public class SpiDisplay : IDisposable
{
    private SpiDevice _spiDevice;
    
    public SpiDisplay(SpiDevice spiDevice)
    {
        _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
    }

    public void WriteData(ReadOnlySpan<byte> data)
    {
        _spiDevice.Write(data);
    }

    public void Dispose()
    {
        _spiDevice?.Dispose();
        _spiDevice = null;
    }
}
```

### 6. Sample Code Guidelines

Sample applications should also follow abstraction principles:

✅ **DO:**

```csharp
// Platform-agnostic sample
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Bmp280;

// Works on any platform with I2C support
I2cConnectionSettings settings = new(busId: 1, deviceAddress: Bmp280.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Bmp280 sensor = new(device);

Console.WriteLine($"Temperature: {sensor.ReadTemperature().DegreesCelsius}°C");
```

❌ **DON'T:**

```csharp
// Platform-specific sample
#if LINUX
    var device = new UnixI2cDevice("/dev/i2c-1", 0x76);
#elif WINDOWS
    var device = new WindowsI2cDevice("I2C1", 0x76);
#endif
```

### 7. Testing Cross-Platform Compatibility

#### Build on Multiple Platforms

```bash
# Test builds on different configurations
dotnet build -c Release

# If possible, test on:
# - Linux (Raspberry Pi, Ubuntu)
# - Windows (Windows IoT, Desktop)
# - macOS (if GPIO simulator available)
```

#### Verify No Platform-Specific References

```bash
# Check project file for platform-specific packages
cat src/devices/<DeviceName>/<DeviceName>.csproj | grep -i "windows\|linux\|unix"

# Should only reference System.Device.* packages
```

### 8. Common Cross-Platform Issues

#### Timing Differences

Some operations may have different timing characteristics across platforms:

✅ **DO:**

```csharp
// Use conservative timing that works everywhere
Thread.Sleep(10); // Safe across platforms

// Or use Stopwatch for precise timing
var sw = Stopwatch.StartNew();
while (sw.ElapsedMilliseconds < 10) { }
```

#### Endianness

Be aware of byte order when working with multi-byte values:

✅ **DO:**

```csharp
// Use BitConverter with explicit endianness
int value = BinaryPrimitives.ReadInt16LittleEndian(buffer);
```

### 9. Documentation Requirements

When code must be platform-specific:

✅ **DO:**

```csharp
/// <summary>
/// Enables high-speed mode. 
/// </summary>
/// <remarks>
/// <para><b>Platform Support:</b></para>
/// <list type="bullet">
/// <item>Linux: Fully supported on kernel 5.0+</item>
/// <item>Windows IoT: Not supported</item>
/// <item>Windows Desktop: Not supported</item>
/// </list>
/// </remarks>
/// <exception cref="PlatformNotSupportedException">
/// Thrown on unsupported platforms
/// </exception>
public void EnableHighSpeedMode()
```

### 10. Review Checklist

Use this when reviewing device binding for hardware abstraction:

- [ ] No `DllImport` or P/Invoke declarations
- [ ] No hardcoded paths (`/sys/`, `/dev/`, `/proc/`)
- [ ] No `RuntimeInformation.IsOSPlatform` checks (unless justified)
- [ ] No `Environment.OSVersion` checks
- [ ] No `Process.Start` for hardware access
- [ ] Uses `System.Device.Gpio` for GPIO
- [ ] Uses `System.Device.I2c` for I²C
- [ ] Uses `System.Device.Spi` for SPI
- [ ] Uses `System.Device.Pwm` for PWM
- [ ] Transport objects (I2C/SPI/GPIO) passed from caller
- [ ] Platform-specific features properly documented
- [ ] Platform-specific features fail gracefully
- [ ] Sample code is platform-agnostic
- [ ] Builds successfully without platform-specific errors

## Common Violations and Fixes

### Violation 1: Direct GPIO Access

```csharp
// ❌ WRONG
File.WriteAllText("/sys/class/gpio/export", "17");
File.WriteAllText("/sys/class/gpio/gpio17/direction", "out");

// ✅ CORRECT
using GpioController controller = new();
controller.OpenPin(17, PinMode.Output);
```

### Violation 2: Platform Detection

```csharp
// ❌ WRONG
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    _gpioPath = "/sys/class/gpio";
}

// ✅ CORRECT - Let abstractions handle it
using GpioController controller = new();
```

### Violation 3: Process Execution

```csharp
// ❌ WRONG
Process.Start("i2cget", "-y 1 0x48");

// ✅ CORRECT
I2cConnectionSettings settings = new(1, 0x48);
using I2cDevice device = I2cDevice.Create(settings);
byte[] data = new byte[1];
device.Read(data);
```

## Quick Validation Script

Save as `check-abstraction.sh`:

```bash
#!/bin/bash
DEVICE_DIR=$1

echo "Checking hardware abstraction compliance for $DEVICE_DIR"
echo ""

echo "Checking for P/Invoke..."
grep -rn "DllImport" "$DEVICE_DIR" || echo "  ✓ No P/Invoke found"

echo ""
echo "Checking for OS detection..."
grep -rn "RuntimeInformation\|OSPlatform\|Environment.OSVersion" "$DEVICE_DIR" || echo "  ✓ No OS detection found"

echo ""
echo "Checking for hardcoded paths..."
grep -rn "/sys/\|/dev/\|/proc/" "$DEVICE_DIR" || echo "  ✓ No hardcoded paths found"

echo ""
echo "Checking for process execution..."
grep -rn "Process.Start" "$DEVICE_DIR" || echo "  ✓ No process execution found"

echo ""
echo "Hardware abstraction check complete!"
```

Usage:

```bash
chmod +x check-abstraction.sh
./check-abstraction.sh src/devices/Bmp280
```

## References

- [System.Device.Gpio Documentation](https://docs.microsoft.com/dotnet/api/system.device.gpio)
- [System.Device.I2c Documentation](https://docs.microsoft.com/dotnet/api/system.device.i2c)
- [Copilot Instructions - Cross-Platform](../../copilot-instructions.md#cross-platform)
- [Device Conventions](../../../Documentation/Devices-conventions.md)
