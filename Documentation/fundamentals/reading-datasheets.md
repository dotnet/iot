# Reading Device Datasheets

Device datasheets contain critical information for interfacing sensors, displays, and other peripherals with your single-board computer. This guide helps you extract the essential information from datasheets.

## What is a Datasheet?

A datasheet is a technical document provided by the device manufacturer that describes:

- Electrical specifications (voltage, current, timing)
- Communication protocols (I2C, SPI, UART addresses and commands)
- Physical specifications (dimensions, pin layouts)
- Operating conditions (temperature, humidity ranges)
- Functional descriptions (what the device does and how)

## Key Sections to Find

### 1. Pin Configuration / Pinout

**What to look for:**

- Pin names and numbers
- Pin functions (power, ground, data, clock, etc.)
- Alternative functions for multi-purpose pins

**Example from a typical sensor:**

```
Pin 1 (VCC):  Power supply, 3.3V-5V
Pin 2 (GND):  Ground
Pin 3 (SCL):  I2C Clock
Pin 4 (SDA):  I2C Data
```

**Why it matters:** Wrong connections can damage your device or Raspberry Pi.

### 2. Electrical Specifications

**What to look for:**

- **Supply voltage range** (e.g., 3.0V - 3.6V or 3.3V - 5V)
- **Logic voltage levels** (HIGH and LOW thresholds)
- **Current consumption** (typical and maximum)
- **Output current capability**

**Example:**

```
Supply Voltage (VCC):      3.3V ±10%
Logic High Input (VIH):    ≥0.7 × VCC
Logic Low Input (VIL):     ≤0.3 × VCC
Maximum Current Draw:      10 mA typical, 50 mA max
```

**Why it matters:**

- Raspberry Pi GPIO outputs 3.3V (not 5V!)
- Some 5V devices may not recognize 3.3V as HIGH
- Exceeding current limits can damage pins

### 3. Communication Protocol Details

#### For I2C Devices

**What to look for:**

- **I2C address** (7-bit or 8-bit notation)
- **Clock speed support** (standard 100kHz, fast 400kHz, etc.)
- **Pull-up resistor requirements**

**Example:**

```
I2C Address: 0x76 (default), can be changed to 0x77 via SDO pin
Clock Speed: Up to 3.4 MHz (High-Speed mode)
Pull-up Resistors: 4.7kΩ recommended for 3.3V
```

**Common confusion:** 7-bit vs 8-bit addresses

- **7-bit address (most common in datasheets):** 0x76
- **8-bit addresses (rare):** 0xEC (write), 0xED (read)
- .NET IoT uses **7-bit addresses**

#### For SPI Devices

**What to look for:**

- **SPI mode** (Mode 0, 1, 2, or 3)
- **Clock frequency** (maximum supported)
- **Bit order** (MSB first or LSB first)
- **CS/SS polarity** (active low or high)

**Example:**

```
SPI Mode: Mode 0 (CPOL=0, CPHA=0)
Maximum Clock: 10 MHz
Bit Order: MSB first
CS: Active Low
```

**Why it matters:** Wrong SPI mode = no communication

#### For UART Devices

**What to look for:**

- **Baud rate** (9600, 115200, etc.)
- **Data bits** (usually 8)
- **Parity** (none, even, odd)
- **Stop bits** (1 or 2)
- **Flow control** (none, hardware, software)

**Example:**

```
Baud Rate: 9600 bps (default), configurable to 115200
Data Format: 8N1 (8 data bits, no parity, 1 stop bit)
Flow Control: None
```

### 4. Register Map / Command Set

**What to look for:**

- **Register addresses** (what to write to where)
- **Bit definitions** (what each bit in a register does)
- **Read/write operations** (which registers are read-only, write-only, or read-write)

**Example from a temperature sensor:**

```
Register 0xF4 (ctrl_meas): Control measurement settings
  Bits 7-5: Temperature oversampling (000 = skip, 001 = ×1, 010 = ×2...)
  Bits 4-2: Pressure oversampling
  Bits 1-0: Mode (00 = sleep, 01/10 = forced, 11 = normal)

Register 0xFA (temp_msb): Temperature MSB [read-only]
Register 0xFB (temp_lsb): Temperature LSB [read-only]
Register 0xFC (temp_xlsb): Temperature XLSB [read-only]
```

**How to use this:**

```csharp
// Write to register 0xF4 to configure sensor
byte[] config = { 0xF4, 0b001_001_11 }; // temp×1, press×1, normal mode
i2cDevice.Write(config);

// Read temperature from registers 0xFA, 0xFB, 0xFC
i2cDevice.WriteByte(0xFA); // Set register pointer
byte[] tempData = new byte[3];
i2cDevice.Read(tempData);
int rawTemp = (tempData[0] << 12) | (tempData[1] << 4) | (tempData[2] >> 4);
```

### 5. Timing Diagrams

**What to look for:**

- **Setup time** (time before clock edge that data must be stable)
- **Hold time** (time after clock edge that data must remain stable)
- **Clock frequency limits**
- **Startup time** (how long after power-on before device is ready)
- **Reset timing** (pulse width requirements)

**Example:**

```
Power-on to ready time: 2 ms typical, 5 ms max
Reset pulse width: Minimum 1 µs
I2C START setup time: 600 ns minimum
```

**Why it matters:** Violating timing can cause communication failures.

### 6. Example Code or Pseudo-code

Some datasheets include example code (often in C):

```c
// Example from datasheet
void init_sensor() {
    write_register(0xF4, 0x27); // Configure oversampling
    write_register(0xF5, 0xA0); // Configure standby time
    delay_ms(10);               // Wait for sensor to stabilize
}
```

**Translate to C#:**

```csharp
void InitSensor(I2cDevice device)
{
    device.Write(new byte[] { 0xF4, 0x27 }); // Configure oversampling
    device.Write(new byte[] { 0xF5, 0xA0 }); // Configure standby time
    Thread.Sleep(10);                         // Wait for sensor to stabilize
}
```

### 7. Typical Application Circuits

Look for reference circuits showing:

- Required external components (resistors, capacitors)
- Pull-up/pull-down resistors
- Bypass/decoupling capacitors
- Crystal oscillators (if needed)

**Example:**

```
        VCC (3.3V)
         │
        ┌┴┐
        │ │ 4.7kΩ pull-up (SCL)
        └┬┘
         │
    ┌────┴────┐
    │   SCL   │
    │         │
    │ Sensor  │
    │         │
    │   SDA   │
    └────┬────┘
         │
        ┌┴┐
        │ │ 4.7kΩ pull-up (SDA)
        └┬┘
         │
        VCC
```

## Common Datasheet Challenges

### 1. Multiple I2C Addresses

Some devices have configurable addresses:

```
I2C Address: 0x76 (SDO to GND) or 0x77 (SDO to VCC)
```

**How to handle:**

- Check your module's schematic or try both addresses
- Use `i2cdetect` tool to scan for devices:

```bash
i2cdetect -y 1
```

### 2. Big-Endian vs Little-Endian

Multi-byte values may be transmitted in different orders:

- **Big-Endian (MSB first):** 0x12 0x34 = 0x1234
- **Little-Endian (LSB first):** 0x34 0x12 = 0x1234

**Check the datasheet:**

```
Temperature data format: MSB first
temp_msb (0xFA): Bits 19-12
temp_lsb (0xFB): Bits 11-4
temp_xlsb (0xFC): Bits 3-0
```

**C# implementation:**

```csharp
// Big-Endian (MSB first)
int value = (msb << 8) | lsb;

// Little-Endian (LSB first)
int value = (lsb << 8) | msb;
```

### 3. Signed vs Unsigned Values

Temperature sensors often use signed integers:

```
Temperature Output: 16-bit signed integer
Range: -40°C to +85°C
```

**C# implementation:**

```csharp
short signedTemp = (short)((msb << 8) | lsb); // Signed 16-bit
double celsius = signedTemp / 100.0; // Convert based on datasheet
```

### 4. Calibration Data

Many sensors require calibration coefficients:

```
Calibration registers: 0x88 to 0xA1
Must be read once at startup and used for all conversions
```

**Always read and store calibration data before measurements.**

### 5. Undocumented Registers

If a register isn't documented:

- Don't write to it (may damage device)
- Don't rely on its value
- Stick to documented registers only

## Practical Example: Reading a BME280 Datasheet

Let's extract key information for a BME280 temperature/humidity sensor:

### Step 1: Find I2C Address

**Datasheet says:** "I2C address is 0x76 or 0x77 depending on SDO pin"

**Your code:**

```csharp
// Try 0x76 first, then 0x77 if it fails
I2cDevice bme280 = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));
```

### Step 2: Find Initialization Sequence

**Datasheet shows:**

1. Read calibration data from registers 0x88-0xA1
2. Write config to register 0xF4
3. Wait for measurement

**Your code:**

```csharp
// Read 26 bytes of calibration data
i2c.WriteByte(0x88);
byte[] calibData = new byte[26];
i2c.Read(calibData);

// Configure: temp oversampling ×1, pressure ×1, normal mode
i2c.Write(new byte[] { 0xF4, 0b001_001_11 });
Thread.Sleep(10);
```

### Step 3: Find Temperature Reading Method

**Datasheet shows:**

- Temperature stored in registers 0xFA, 0xFB, 0xFC
- 20-bit value, MSB first
- Requires calibration compensation

**Your code:**

```csharp
i2c.WriteByte(0xFA);
byte[] tempData = new byte[3];
i2c.Read(tempData);

int rawTemp = (tempData[0] << 12) | (tempData[1] << 4) | (tempData[2] >> 4);
// Apply calibration (see datasheet for formula)
```

## Tools for Verification

### Hardware Tools

1. **Multimeter** - Verify voltages and continuity
2. **Logic Analyzer** - Capture and decode I2C/SPI signals
3. **Oscilloscope** - Check signal quality and timing

### Software Tools

1. **i2cdetect** - Scan I2C bus for devices

```bash
i2cdetect -y 1
```

2. **i2cdump** - Read all registers from I2C device

```bash
i2cdump -y 1 0x76
```

3. **gpioinfo** - List available GPIO lines

```bash
gpioinfo
```

## Tips for Success

1. **Read the entire datasheet** - Don't skip sections
2. **Verify electrical compatibility** - Check voltage levels first
3. **Start with simple tests** - Read device ID register first
4. **Compare with existing code** - Check device bindings in [src/devices](../../src/devices)
5. **Use diagnostic tools** - Verify hardware before debugging code
6. **Check errata documents** - Manufacturers publish bug fixes and workarounds
7. **Join communities** - Forums and GitHub issues often have solutions

## Common Datasheet Sections

| Section Name | What to Look For |
|--------------|------------------|
| Features | High-level capabilities |
| Absolute Maximum Ratings | Don't exceed these! |
| Electrical Characteristics | Voltage, current, timing specs |
| Pin Configuration | Pin numbers and functions |
| Functional Description | How the device works |
| Application Information | Example circuits |
| Register Map | Addresses and bit definitions |
| Package Information | Physical dimensions |

## When Datasheets Are Unclear

1. **Search for application notes** - Manufacturers often publish additional documents
2. **Check for Arduino libraries** - Even if you use C#, Arduino code shows working examples
3. **Look for community drivers** - Check [Adafruit](https://github.com/adafruit), [SparkFun](https://github.com/sparkfun)
4. **Ask on forums** - [Electronics StackExchange](https://electronics.stackexchange.com/), [Raspberry Pi Forums](https://forums.raspberrypi.com/)
5. **Check existing bindings** - [.NET IoT Device Bindings](../../src/devices) may already support your device

## Example: Quick Datasheet Summary Template

When starting with a new device, create a quick reference:

```
Device: BME280 Temperature/Humidity/Pressure Sensor
Protocol: I2C
I2C Address: 0x76 or 0x77
Voltage: 3.3V (1.8V-3.6V)
Current: 3.6 µA typical
Timing: 2ms startup time

Key Registers:
- 0xD0: Chip ID (should read 0x60)
- 0x88-0xA1: Calibration data (read once at startup)
- 0xF4: Control measurement (write to configure)
- 0xFA-0xFC: Temperature data (read after measurement)

Initialization:
1. Read chip ID to verify device
2. Read calibration data
3. Write config to 0xF4
4. Wait 10ms
5. Read temperature from 0xFA-0xFC
```

## Next Steps

- [GPIO Basics](gpio-basics.md) - Understand pin functions
- [Understanding Protocols](understanding-protocols.md) - Learn I2C, SPI, UART
- [Device Bindings](../../src/devices/README.md) - See example implementations
- [Troubleshooting](../troubleshooting.md) - Debug common issues

## Additional Resources

- [How to Read a Datasheet - SparkFun](https://learn.sparkfun.com/tutorials/how-to-read-a-datasheet)
- [Understanding Datasheets - Adafruit](https://learn.adafruit.com/understanding-electronic-component-datasheets)
- [Electronics Basics - All About Circuits](https://www.allaboutcircuits.com/textbook/)
