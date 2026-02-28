---
name: fix-api-conventions
description: Guide for ensuring device bindings follow .NET IoT API conventions and design patterns
license: MIT
---

## Purpose

This skill helps Copilot agents review and fix device bindings to ensure they follow the repository's established API conventions, naming patterns, and design principles.

## When to Use This Skill

- Reviewing existing device binding code for convention compliance
- Fixing API design issues in device bindings
- Code review identifies convention violations
- Modernizing older device bindings to match current patterns

## Instructions

### 1. API Naming Conventions

#### Methods vs Properties

- **Methods** (e.g., `ReadTemperature()`, `GetTemperature()`) for values that may change between calls
- **Properties** (e.g., `SomeRegister`) for values that don't change (except via setter)

#### Naming Patterns

- Follow .NET naming rules: PascalCase for public members/types, camelCase for locals/fields
- Interfaces start with `I` (e.g., `ITemperatureSensor`)
- Async methods must have `Async` suffix and synchronous equivalent

#### Read Methods

✅ **DO:**

```csharp
public bool TryReadTemperature(out Temperature temperature)
public Temperature ReadTemperature()
public bool TryGetPressure(out Pressure pressure)
```

❌ **DON'T:**

```csharp
public double GetTemp() // Use Temperature type, not double
public double ReadTemperature() // Should return Temperature or use Try pattern
public Temperature? ReadTemperature() // Use Try pattern instead of nullable
```

### 2. Value Types and Units

#### Use UnitsNet Types

✅ **DO:**

```csharp
using UnitsNet;

public bool TryReadTemperature(out Temperature temperature)
public Pressure ReadPressure()
public ElectricPotential ReadVoltage()
```

❌ **DON'T:**

```csharp
public double GetTemperatureCelsius() // Use Temperature type
public float GetPressurePa() // Use Pressure type
```

#### When UnitsNet Not Available

- Use SI units (International System of Units)
- Document units clearly in XML comments

```csharp
/// <summary>
/// Reads humidity in percent (0-100).
/// </summary>
/// <returns>Relative humidity in %RH</returns>
public double ReadHumidity()
```

#### Handling Invalid Values

✅ **DO:**

```csharp
public bool TryReadTemperature(out Temperature temperature)
{
    // Return false on failure, set valid value on success
    if (/* read failed */)
    {
        temperature = default;
        return false;
    }
    temperature = Temperature.FromDegreesCelsius(value);
    return true;
}
```

❌ **DON'T:**

```csharp
public double ReadTemperature()
{
    return double.NaN; // Don't use sentinel values
}
```

### 3. Constructor Design

#### Required Parameters Only

- Constructor should only require parameters device cannot work without
- Everything else should have default values
- Use Settings class for devices with many parameters

✅ **DO:**

```csharp
public Bmp280(I2cDevice i2cDevice, I2cAddress address = I2cAddress.Primary)
public Dht22(int pin, GpioController? controller = null, bool shouldDispose = true)
```

❌ **DON'T:**

```csharp
public Bmp280(I2cDevice i2cDevice, int pollingDelay, bool enableFilter, 
              Oversampling tempOversampling, Oversampling pressOversampling)
// Too many required parameters - use Settings class
```

#### Integer Invalid Values

- Use `-1` for invalid/unassigned integer values (not `null` or `Nullable<int>`)

```csharp
public int Pin { get; set; } = -1; // -1 indicates unassigned
```

### 4. Resource Management

#### IDisposable Implementation

MUST implement IDisposable if device owns hardware resources.

✅ **DO:**

```csharp
public class Bmp280 : IDisposable
{
    private I2cDevice? _i2cDevice;
    private bool _disposed;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

#### Transport Ownership

- **I2cDevice, SpiDevice, PwmChannel**: 1:1 with hardware, **should be disposed by device**
- **GpioController**: Can be shared, **add optional `shouldDispose` flag** (default true)

✅ **DO:**

```csharp
public Dht22(int pin, GpioController? controller = null, bool shouldDispose = true)
{
    _pin = pin;
    _shouldDispose = shouldDispose || controller is null;
    _controller = controller ?? new GpioController();
}

public void Dispose()
{
    if (_shouldDispose)
    {
        _controller?.Dispose();
    }
    _controller = null;
}
```

### 5. Register and Address Management

#### Use Enums for Registers

✅ **DO:**

```csharp
private enum Register : byte
{
    ChipId = 0xD0,
    Reset = 0xE0,
    Status = 0xF3,
    Control = 0xF4,
    Config = 0xF5,
    PressureData = 0xF7,
    TemperatureData = 0xFA
}

// Usage
byte value = Read(Register.ChipId);
```

❌ **DON'T:**

```csharp
private const byte CHIP_ID_REG = 0xD0; // Use enum instead
byte value = Read(0xD0); // Magic number
```

### 6. Error Handling

#### Input Validation

✅ **DO:**

```csharp
public Sensor(int pin)
{
    if (pin < 0)
        throw new ArgumentOutOfRangeException(nameof(pin), "Pin must be non-negative");
    _pin = pin;
}

public void SetSamplingRate(int rate)
{
    if (rate < 1 || rate > 100)
        throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be 1-100 Hz");
    _samplingRate = rate;
}
```

#### Hardware Failures

✅ **DO:**

```csharp
public bool TryReadTemperature(out Temperature temperature)
{
    try
    {
        byte[] data = ReadRegister(Register.TempData);
        if (!ValidateChecksum(data))
        {
            temperature = default;
            return false;
        }
        temperature = ParseTemperature(data);
        return true;
    }
    catch (IOException)
    {
        temperature = default;
        return false;
    }
}
```

❌ **DON'T:**

```csharp
public Temperature ReadTemperature()
{
    try
    {
        return ParseTemperature(ReadRegister(Register.TempData));
    }
    catch
    {
        return Temperature.FromDegreesCelsius(0); // Bogus data
    }
}
```

### 7. Visibility and Access Modifiers

#### Public API Minimalism

- Only most useful APIs should be public
- Advanced features should be `protected` (allows inheritance)
- Internal implementation details should be `private` or `internal`

✅ **DO:**

```csharp
public class Sensor
{
    // Main functionality - public
    public bool TryReadTemperature(out Temperature temp) { }
    
    // Advanced calibration - protected
    protected void SetCalibrationValue(int index, double value) { }
    
    // Internal details - private
    private byte[] ReadRawData() { }
}
```

### 8. Documentation Requirements

#### XML Comments

✅ **DO:**

```csharp
/// <summary>
/// Reads the current temperature from the sensor.
/// </summary>
/// <param name="temperature">The temperature in degrees Celsius</param>
/// <returns>True if read successful, false on communication error</returns>
/// <remarks>
/// Reading takes approximately 10ms. Values range from -40°C to +85°C.
/// </remarks>
public bool TryReadTemperature(out Temperature temperature)
```

### 9. Common Anti-Patterns to Fix

❌ **Console I/O in Library Code**

```csharp
public void ReadSensor()
{
    var temp = ReadTemperature();
    Console.WriteLine($"Temp: {temp}"); // Move to sample, not library
}
```

❌ **Thread-Static Caches**

```csharp
[ThreadStatic]
private static byte[]? _buffer; // Hidden state, avoid
```

❌ **Background Threads Without Clear Ownership**

```csharp
private Thread? _pollingThread; // Ensure proper disposal and control
```

❌ **Board-Specific Hardcoded Paths**

```csharp
const string GpioPath = "/sys/class/gpio"; // Use abstractions
```

## Review Checklist

Use this when reviewing device binding APIs:

- [ ] Methods/properties used appropriately
- [ ] UnitsNet types used for physical quantities
- [ ] Try* pattern for operations that may fail
- [ ] No sentinel values (NaN, null) for failures
- [ ] Constructor has only required parameters
- [ ] IDisposable implemented correctly
- [ ] Transport ownership handled properly
- [ ] Enums used for registers/addresses
- [ ] Input validation with appropriate exceptions
- [ ] XML documentation with units
- [ ] Public API is minimal and focused
- [ ] No console I/O in library code
- [ ] No platform-specific code

## References

- [Device Conventions Document](../../../Documentation/Devices-conventions.md)
- [Copilot Instructions - API Design](../../copilot-instructions.md#device-binding-design)
- [UnitsNet Documentation](https://github.com/angularsen/UnitsNet)
