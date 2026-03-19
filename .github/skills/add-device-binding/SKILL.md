---
name: add-device-binding
description: Guide for adding new IoT device bindings to the repository following .NET IoT conventions
license: MIT
---

## Purpose

This skill helps Copilot agents create new IoT device bindings (sensors, displays, motors, etc.) that follow the repository's established patterns and conventions for System.Device.Gpio and Iot.Device.Bindings.

## When to Use This Skill

- Developer requests adding a new device binding for a sensor, display, motor, or other hardware component
- Creating a new driver under `src/devices/<DeviceName>`
- Need to ensure the device follows .NET IoT conventions and patterns

## Instructions

### 1. Device Structure Setup

Create the following directory structure under `src/devices/<DeviceName>`:

```text
src/devices/<DeviceName>/
├── <DeviceName>.csproj
├── <DeviceName>.cs (main class)
├── samples/
│   ├── <DeviceName>.Samples.csproj
│   └── Program.cs
├── tests/ (if applicable)
│   └── <DeviceName>.Tests.csproj
└── README.md
```

### 2. Constructor & Dependencies

- Accept hardware transports from the caller: `I2cDevice`, `SpiDevice`, `GpioController`, pin numbers, bus addresses
- Provide sensible defaults (e.g., common I²C addresses like 0x48)
- **Never hardcode board-specific details**
- Avoid singletons and global state

**Example Constructor:**

```csharp
public Bmp280(I2cDevice i2cDevice, I2cAddress address = I2cAddress.Primary)
{
    _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
    _address = address;
}
```

### 3. API Design Patterns

- Use **`TryRead*` methods** that return `bool` and set `out` parameters for sensor readings

  ```csharp
  public bool TryReadTemperature(out Temperature temperature)
  ```


- Use **UnitsNet** types for physical quantities (Temperature, Pressure, Length, etc.)
- Properties for values that don't change between calls
- Methods for values that may change
- Use enums/flags for registers, modes, scales
- Keep synchronous APIs predictable; only use `async` if genuine I/O concurrency needed

### 4. Resource Management (CRITICAL)

- **MUST implement `IDisposable`**
- Dispose hardware resources (I2C/SPI devices, GPIO pins) in the Dispose method
- If transport is passed by caller, dispose it unless `shouldDispose` flag says otherwise
- Design for failure: ensure exceptions leave device in consistent state

**Example Disposal:**

```csharp
protected virtual void Dispose(bool disposing)
{
    if (_disposed)
        return;

    if (disposing)
    {
        _i2cDevice?.Dispose();
        _i2cDevice = null;
    }

    _disposed = true;
}
```

### 5. Error Handling

- Guard against misuse with `ArgumentException`/`ArgumentNullException`
- For transient hardware issues, throw or return `false` in `Try*` patterns
- **Never return bogus data silently**
- Validate checksums/status bits where appropriate

### 6. Documentation Requirements

- XML documentation comments on all public APIs
- **Include units** in comments (°C, Pa, %RH, lux, etc.)
- Document parameter ownership (does caller own the transport?)
- README.md with:
  - Device description and capabilities
  - Wiring diagram or connection instructions
  - Required pull-ups/pull-downs
  - Expected ranges and units
  - Code example demonstrating usage

### 7. Sample Application

Create a runnable sample in `samples/Program.cs`:

- Keep it simple and readable
- Demonstrate typical usage patterns
- Show proper disposal (using statements)
- Handle exceptions gracefully
- Don't hardcode board-specific paths

**Example Sample:**

```csharp
using System;
using System.Device.I2c;
using Iot.Device.Bmp280;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bmp280.DefaultI2cAddress));
using Bmp280 sensor = new(i2cDevice);

while (true)
{
    if (sensor.TryReadTemperature(out Temperature temp))
    {
        Console.WriteLine($"Temperature: {temp.DegreesCelsius:F2} °C");
    }
    Thread.Sleep(1000);
}
```

### 8. Cross-Platform Considerations

- Use **System.Device.*** abstractions exclusively
- No P/Invoke or OS-specific syscalls
- No hardcoded paths to `/sys` or `/proc`
- If a feature is platform-only, guard it and fail gracefully

### 9. Testing

- Add unit tests for logic that can be isolated from hardware
- Focus on: framing/parsing functions, calculations, protocol logic
- Hardware-dependent tests should be clearly marked
- Ensure samples compile even without hardware

### 10. Build Validation

After creating the device:

```bash
cd src/devices/<DeviceName>
../../../dotnet.sh build

cd samples
../../../../dotnet.sh build

# Check for hardware abstraction violations
grep -r "Environment.OSVersion\|DllImport\|P/Invoke" .
```

## Review Checklist

Before completing:

- [ ] Public API follows repo naming/style
- [ ] Members are XML-documented with units (°C, Pa, %RH, lux, etc.)
- [ ] Constructor accepts caller-provided transports
- [ ] Implements `IDisposable`; all hardware resources released
- [ ] Uses enums/consts for registers/addresses
- [ ] Validates timing and checksums
- [ ] `Try*` methods return accurate success/failure
- [ ] Adds sample application
- [ ] Adds device README with wiring info
- [ ] Adds tests for hardware-independent logic
- [ ] Builds successfully without errors
- [ ] No platform-specific code or P/Invoke

## Examples

### Good Device Binding Example

```csharp
public class Dht22 : IDisposable
{
    private GpioController? _controller;
    private readonly int _pin;
    private readonly bool _shouldDispose;

    public Dht22(int pin, GpioController? controller = null, bool shouldDispose = true)
    {
        _pin = pin;
        _shouldDispose = shouldDispose || controller is null;
        _controller = controller ?? new GpioController();
    }

    public bool TryReadTemperature(out Temperature temperature)
    {
        // Implementation that validates data and checksums
    }

    public void Dispose()
    {
        if (_shouldDispose)
        {
            _controller?.Dispose();
        }
        _controller = null;
    }
}
```

## References

- [Device Conventions](../../../Documentation/Devices-conventions.md)
- [Contributing Guidelines](../../../Documentation/CONTRIBUTING.md)
- [Copilot Instructions](../../copilot-instructions.md)
- [UnitsNet Library](https://github.com/angularsen/UnitsNet)
