---
name: update-device-documentation
description: Guide for creating and maintaining comprehensive documentation for IoT device bindings
license: MIT
---

## Purpose

This skill helps Copilot agents create and maintain high-quality documentation for device bindings, including README files, XML documentation, wiring diagrams, and usage examples.

## When to Use This Skill

- Adding documentation for a new device binding
- Updating existing device documentation
- Adding wiring diagrams or connection instructions
- Improving code examples in README files
- Adding XML documentation comments to APIs

## Instructions

### 1. Device README Structure

Each device binding must have a `README.md` in its directory with these sections:

```markdown
# DeviceName - Brief Description

Brief overview of what the device does (1-2 sentences).

## Documentation

Link to device datasheet/specification.

## Usage

Basic code example showing typical usage.

## Binding Notes

Important information about this specific binding:
- Default I2C/SPI addresses
- Required configuration
- Known limitations
- Platform-specific considerations

## References

Links to:
- Datasheet
- Device manufacturer page
- Related bindings
```

### 2. Writing Effective Device Documentation

#### Device Overview Section

✅ **DO:**

```markdown
# BME280 - Combined Temperature, Pressure, and Humidity Sensor

The BME280 is a combined digital humidity, pressure and temperature sensor 
manufactured by Bosch. It provides high precision measurements over I2C or SPI.
```

Include:

- Full device name and manufacturer
- What the device measures/does
- Communication protocol(s) supported

#### Documentation Section

✅ **DO:**

```markdown
## Documentation

- BME280 [datasheet](https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bme280-ds002.pdf)
```

Always link to official datasheets and specifications.

### 3. Usage Examples

#### Code Example Requirements

- Show complete, runnable code
- Use `using` statements for proper disposal
- Include necessary namespace imports
- Show error handling
- Add comments explaining key steps
- Don't hardcode board-specific values

✅ **DO:**

```csharp
## Usage

**Important**: Make sure you properly setup the I2C pins for your device.

```csharp
using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet;

// I2C address 0x76 is default, use 0x77 if SDO pin is high
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bme280.DefaultI2cAddress));
using Bme280 bme280 = new(i2cDevice);

// Set sampling and filtering
bme280.TemperatureSampling = Sampling.Standard;
bme280.PressureSampling = Sampling.Standard;
bme280.HumiditySampling = Sampling.Standard;

while (true)
{
    // Read sensor values
    if (bme280.TryReadTemperature(out Temperature temperature))
    {
        Console.WriteLine($"Temperature: {temperature.DegreesCelsius:F2} °C");
    }

    if (bme280.TryReadPressure(out Pressure pressure))
    {
        Console.WriteLine($"Pressure: {pressure.Hectopascals:F2} hPa");
    }

    if (bme280.TryReadHumidity(out RelativeHumidity humidity))
    {
        Console.WriteLine($"Humidity: {humidity.Percent:F2} %");
    }

    Thread.Sleep(1000);
}
\`\`\`
```

### 4. Wiring Information

#### Wiring Diagram Requirements

Include wiring information showing:

- Pin connections between device and board
- Required pull-up/pull-down resistors
- Power supply requirements
- Common connection mistakes to avoid

✅ **DO:**

```markdown
## Wiring

### I2C Connection

| BME280 Pin | Raspberry Pi Pin |
|------------|------------------|
| VCC        | 3.3V (Pin 1)     |
| GND        | Ground (Pin 6)   |
| SCL        | GPIO3/SCL (Pin 5)|
| SDA        | GPIO2/SDA (Pin 3)|
| SDO        | Ground or 3.3V   |

**Note:** 
- Pull-up resistors (4.7kΩ) are typically required on SCL and SDA lines
- SDO pin sets I2C address: Low = 0x76, High = 0x77
- Use 3.3V power only, do not connect to 5V
```

### 5. Binding Notes Section

Document important implementation details:

✅ **DO:**

```markdown
## Binding Notes

### Default Addresses
- Primary I2C address: 0x76 (SDO pin grounded)
- Secondary I2C address: 0x77 (SDO pin to VCC)

### Measurement Timing
- Single measurement takes approximately 10ms
- Continuous mode updates at configured rate (0.5Hz to 16Hz)

### Calibration
- Device includes factory calibration data
- Calibration is automatically read and applied by the binding

### Known Limitations
- SPI mode not yet implemented
- Forced mode not supported, use normal mode instead
```

### 6. XML Documentation for APIs

#### Public API Documentation

Every public member needs XML documentation:

✅ **DO:**

```csharp
/// <summary>
/// Reads the current temperature from the BME280 sensor.
/// </summary>
/// <param name="temperature">
/// When this method returns true, contains the measured temperature.
/// When false, contains the default value.
/// </param>
/// <returns>
/// True if the temperature was read successfully; false if communication
/// error occurred or data was invalid.
/// </returns>
/// <remarks>
/// Temperature range: -40°C to +85°C with ±1°C accuracy.
/// Reading takes approximately 10ms in standard sampling mode.
/// </remarks>
public bool TryReadTemperature(out Temperature temperature)
```

#### Include Units in Documentation

Always specify units:

```csharp
/// <summary>
/// Gets or sets the temperature oversampling rate.
/// Higher values provide better accuracy but slower measurements.
/// </summary>
/// <value>Oversampling multiplier (1x, 2x, 4x, 8x, or 16x)</value>
public Sampling TemperatureSampling { get; set; }
```

### 7. Property and Enum Documentation

Document ranges and valid values:

✅ **DO:**

```csharp
/// <summary>
/// Measurement sampling rate options.
/// </summary>
public enum Sampling
{
    /// <summary>
    /// Measurement skipped (0x)
    /// </summary>
    Skipped = 0b000,

    /// <summary>
    /// Oversampling x1 - fastest, least accurate
    /// </summary>
    UltraLowPower = 0b001,

    /// <summary>
    /// Oversampling x2
    /// </summary>
    LowPower = 0b010,

    /// <summary>
    /// Oversampling x4 (recommended for most applications)
    /// </summary>
    Standard = 0b011
}
```

### 8. Markdown Linting

Ensure documentation passes markdown linting:

```bash
# Install markdownlint
npm install -g markdownlint-cli

# Check device README
markdownlint -c .markdownlint.json src/devices/Bme280/README.md

# Fix common issues:
# - No trailing spaces
# - Blank line after headers
# - Proper list formatting
# - No bare URLs (use [text](url) format)
```

### 9. Common Documentation Patterns

#### For Sensor Devices

```markdown
## Specifications

- Measurement range: -40°C to +85°C
- Accuracy: ±1°C
- Resolution: 0.01°C
- Supply voltage: 1.8V to 3.6V
- Interface: I2C (up to 3.4 MHz) or SPI (up to 10 MHz)
```

#### For Display Devices

```markdown
## Specifications

- Display size: 128x64 pixels
- Colors: Monochrome (white on black)
- Interface: I2C or SPI
- Typical current: 20mA at 3.3V
```

#### For Motor/Actuator Devices

```markdown
## Specifications

- Control interface: PWM
- Operating voltage: 5-12V
- Maximum current: 2A per channel
- PWM frequency: 1kHz to 10kHz recommended
```

### 10. Documentation Review Checklist

Before completing documentation:

- [ ] README.md exists in device directory
- [ ] Device name and description are clear
- [ ] Link to datasheet/specification included
- [ ] Code example is complete and runnable
- [ ] Code example uses proper disposal (`using` statements)
- [ ] Wiring diagram or connection table included
- [ ] Required pull-ups/resistors documented
- [ ] Default I2C/SPI addresses documented
- [ ] Known limitations documented
- [ ] All public APIs have XML documentation
- [ ] Units specified in XML comments (°C, Pa, %RH, etc.)
- [ ] Markdown linting passes
- [ ] No hardcoded board-specific paths in examples
- [ ] Cross-platform considerations mentioned if applicable

## Examples

### Complete Example: Simple Sensor

```markdown
# DHT22 - Temperature and Humidity Sensor

DHT22 is a low-cost digital temperature and humidity sensor that uses a 
one-wire protocol for communication.

## Documentation

- DHT22 [datasheet](https://www.sparkfun.com/datasheets/Sensors/Temperature/DHT22.pdf)

## Usage

```csharp
using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.DHTxx;
using UnitsNet;

using GpioController controller = new();
using Dht22 sensor = new(pin: 4, controller);

while (true)
{
    if (sensor.TryReadTemperature(out Temperature temperature) &&
        sensor.TryReadHumidity(out RelativeHumidity humidity))
    {
        Console.WriteLine($"Temp: {temperature.DegreesCelsius:F1}°C, " +
                         $"Humidity: {humidity.Percent:F1}%");
    }
    else
    {
        Console.WriteLine("Read failed");
    }

    Thread.Sleep(2000); // DHT22 requires 2 second interval
}
\`\`\`

## Wiring

Connect DHT22 to Raspberry Pi:
- Pin 1 (VCC) to 3.3V
- Pin 2 (DATA) to GPIO4 with 4.7kΩ pull-up to 3.3V
- Pin 4 (GND) to Ground

## Binding Notes

- Minimum reading interval: 2 seconds
- Pull-up resistor (4.7kΩ) required on data line
- Temperature range: -40°C to +80°C (±0.5°C accuracy)
- Humidity range: 0-100% RH (±2-5% accuracy)
```

## References

- [Device Conventions](https://github.com/dotnet/iot/blob/main/Documentation/Devices-conventions.md)
- [Contributing Guidelines](https://github.com/dotnet/iot/blob/main/Documentation/CONTRIBUTING.md)
- [Copilot Instructions](https://github.com/dotnet/iot/blob/main/.github/copilot-instructions.md)
- [Markdown Style Guide](https://github.com/dotnet/iot/blob/main/.markdownlint.json)
