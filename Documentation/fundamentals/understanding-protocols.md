# Understanding Communication Protocols

When interfacing with sensors, displays, and other peripherals, you'll use different communication protocols. This guide helps you understand when to use each protocol and their characteristics.

## Quick Comparison

| Protocol | Speed | Wiring | Devices | Best For |
|----------|-------|--------|---------|----------|
| **GPIO** | N/A | 1 wire/device | 1 per pin | Simple on/off, buttons, LEDs |
| **I2C** | Slow-Medium (100-400 kHz) | 2 wires (SDA, SCL) | 100+ (addressable) | Sensors, small displays, multi-device |
| **SPI** | Fast (10+ MHz) | 4+ wires (MOSI, MISO, SCK, CS) | Limited by CS pins | Displays, high-speed sensors, SD cards |
| **UART/Serial** | Medium (9600-115200 baud) | 2 wires (TX, RX) | 1 per port | GPS modules, GSM, Bluetooth, debugging |
| **PWM** | N/A | 1 wire/device | 1 per pin | LED dimming, motor speed, servos |
| **1-Wire** | Slow | 1 wire + ground | Many (addressable) | Temperature sensors (DS18B20) |

## GPIO (General-Purpose Input/Output)

### When to Use

- Simple on/off control (LEDs, relays)
- Reading digital states (buttons, switches)
- No data transfer needed

### Characteristics

- **Pros:** Simple, direct, no protocol overhead
- **Cons:** One pin per device, limited to on/off states

### Example

```csharp
using GpioController controller = new();
controller.OpenPin(18, PinMode.Output);
controller.Write(18, PinValue.High);
```

[Learn more about GPIO](gpio-basics.md)

## I2C (Inter-Integrated Circuit)

### When to Use

- Multiple sensors on the same bus
- Sensors near the controller (< 1 meter)
- Low to medium speed requirements
- Limited pin availability

### Characteristics

- **Pros:** Only 2 wires for many devices, built-in addressing, widely supported
- **Cons:** Relatively slow, limited cable length, address conflicts possible

### Typical Devices

- Temperature/humidity sensors (BME280, SHT31)
- Small OLED displays (SSD1306)
- Real-time clocks (DS3231)
- Port expanders (MCP23017)
- ADCs (ADS1115)

### How It Works

- **Master-slave** architecture (Raspberry Pi is master)
- **Two wires:** SDA (data), SCL (clock)
- **7-bit addresses:** Each device has a unique address (0x08 to 0x7F)
- **Pull-up resistors required** (often built-in on sensor modules)

### Example

```csharp
using System.Device.I2c;

// Bus 1, device address 0x76
I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));

// Write command
device.WriteByte(0xF4);

// Read data
byte[] buffer = new byte[2];
device.Read(buffer);
```

### Common Issues

- **Address conflicts:** Two devices with the same address
- **Missing pull-ups:** SDA/SCL lines need pull-up resistors (typically 4.7kΩ)
- **Speed issues:** Some devices don't support fast mode (400 kHz)

[Learn more about I2C setup](../protocols/i2c.md)

## SPI (Serial Peripheral Interface)

### When to Use

- High-speed data transfer needed
- Displays requiring fast refresh
- SD card access
- When I2C speed is insufficient

### Characteristics

- **Pros:** Very fast, full-duplex (simultaneous read/write), simple protocol
- **Cons:** More wires, limited by available CS pins, no built-in addressing

### Typical Devices

- Large displays (TFT, e-paper)
- High-speed ADCs
- SD/microSD cards
- NRF24L01 wireless modules
- MAX7219 LED matrix drivers

### How It Works

- **Master-slave** architecture
- **Four wires (minimum):**
  - MOSI (Master Out Slave In) - data from master
  - MISO (Master In Slave Out) - data to master
  - SCK/SCLK (Serial Clock) - clock signal
  - CS/SS (Chip Select) - selects active device
- **Multiple devices:** Shared MOSI/MISO/SCK, separate CS for each

### Example

```csharp
using System.Device.Spi;

SpiDevice device = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 1_000_000, // 1 MHz
    Mode = SpiMode.Mode0
});

// Write and read simultaneously
byte[] writeBuffer = { 0x01, 0x02, 0x03 };
byte[] readBuffer = new byte[3];
device.TransferFullDuplex(writeBuffer, readBuffer);
```

### SPI Modes

| Mode | CPOL | CPHA | Clock Idle | Sampling Edge |
|------|------|------|------------|---------------|
| Mode0 | 0 | 0 | Low | Rising |
| Mode1 | 0 | 1 | Low | Falling |
| Mode2 | 1 | 0 | High | Falling |
| Mode3 | 1 | 1 | High | Rising |

**Most devices use Mode0** - check your device datasheet.

[Learn more about SPI setup](../protocols/spi.md)

## UART/Serial (Universal Asynchronous Receiver/Transmitter)

### When to Use

- Communication with GPS modules
- Bluetooth/WiFi modules
- GSM/cellular modems
- Serial consoles and debugging
- RS232/RS485 industrial devices

### Characteristics

- **Pros:** Widely supported, simple point-to-point, good for streaming data
- **Cons:** Two wires per device, no built-in addressing, requires baud rate matching

### Typical Devices

- GPS modules (Neo-6M, Neo-7M)
- Bluetooth modules (HC-05, HC-06)
- GSM modules (SIM800, SIM900)
- LoRa modules (E32, E22)
- Serial sensors (PM2.5 sensors, fingerprint readers)

### How It Works

- **Two wires:** TX (transmit), RX (receive)
- **Asynchronous:** No shared clock, both sides agree on baud rate
- **Baud rate:** Speed in bits per second (common: 9600, 115200)
- **Frame format:** Start bit, data bits (8), optional parity, stop bits

### Example

```csharp
using System.IO.Ports;

SerialPort port = new SerialPort("/dev/ttyS0", 9600);
port.Open();

// Write data
port.Write("AT\r\n");

// Read response
string response = port.ReadLine();
Console.WriteLine(response);

port.Close();
```

**Note:** .NET SerialPort API is used for UART/Serial, not System.Device.* APIs.

### RS232 vs RS485 vs TTL

- **TTL:** 3.3V/5V logic levels (what Raspberry Pi GPIO uses)
- **RS232:** ±12V levels, longer distances, requires level shifter (MAX3232)
- **RS485:** Differential signaling, very long distances (1000m+), multi-drop capable

[Learn more about UART setup](../protocols/uart.md)

## PWM (Pulse Width Modulation)

### When to Use

- Controlling LED brightness
- Motor speed control
- Servo positioning
- Analog-like output from digital pin

### Characteristics

- **Pros:** Smooth control, efficient, widely supported
- **Cons:** Requires PWM-capable pins, frequency limitations

### Typical Uses

- LED dimming (fade effects)
- DC motor speed control
- Servo motors (hobby servos, SG90)
- Buzzer tone generation
- Analog output simulation

### How It Works

- Rapidly toggles pin HIGH/LOW
- **Duty cycle:** Percentage of time HIGH (0-100%)
- **Frequency:** How many times per second to toggle (Hz)

### Example

```csharp
using System.Device.Pwm;

// Chip 0, Channel 0, 400 Hz, 50% duty cycle
PwmChannel pwm = PwmChannel.Create(0, 0, 400, 0.5);
pwm.Start();

// Fade LED from 0% to 100%
for (double duty = 0.0; duty <= 1.0; duty += 0.01)
{
    pwm.DutyCycle = duty;
    Thread.Sleep(20);
}

pwm.Stop();
```

[Learn more about PWM setup](../protocols/pwm.md)

## 1-Wire

### When to Use

- Multiple temperature sensors (DS18B20)
- iButton authentication
- Simple sensor networks

### Characteristics

- **Pros:** Only one data wire, many devices, long distances (100m+)
- **Cons:** Slow, limited device types, requires kernel driver

### Typical Devices

- DS18B20 temperature sensor
- DHT11/DHT22 (uses modified 1-wire protocol)

### How It Works

- **One data wire** + ground
- Each device has unique 64-bit address
- Bus powered or external power
- Requires 4.7kΩ pull-up resistor

**Note:** 1-Wire devices typically use Linux kernel drivers, accessed through `/sys/bus/w1/devices/`.

## Decision Guide

### Choose I2C when:

- Connecting multiple sensors
- Using pre-made sensor modules
- Space/pin count is limited
- Low to medium speed is acceptable
- NOT for: High-speed displays, long distances

### Choose SPI when:

- High-speed data transfer needed
- Large displays (TFT, e-paper)
- SD cards or flash memory
- Full-duplex communication needed
- NOT for: Limited pins, multiple sensors

### Choose UART when:

- Point-to-point communication
- GPS, Bluetooth, GSM modules
- Long-distance RS485 networks
- Streaming data (logging, telemetry)
- NOT for: Multiple devices on one bus

### Choose GPIO when:

- Simple on/off control
- Button input
- One device per pin acceptable
- NOT for: Complex data, multiple devices

## Mixing Protocols

It's common to use multiple protocols in one project:

```csharp
// I2C sensors
I2cDevice tempSensor = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));

// SPI display
SpiDevice display = SpiDevice.Create(new SpiConnectionSettings(0, 0));

// GPIO buttons and LEDs
using GpioController gpio = new();
gpio.OpenPin(17, PinMode.InputPullUp);
gpio.OpenPin(18, PinMode.Output);

// UART GPS
SerialPort gps = new SerialPort("/dev/ttyS0", 9600);
```

## Next Steps

- [I2C Setup Guide](../protocols/i2c.md)
- [SPI Setup Guide](../protocols/spi.md)
- [PWM Setup Guide](../protocols/pwm.md)
- [UART Setup Guide](../protocols/uart.md)
- [GPIO Basics](gpio-basics.md)

## Additional Resources

- [I2C Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/i2c/all)
- [SPI Tutorial - SparkFun](https://learn.sparkfun.com/tutorials/serial-peripheral-interface-spi/all)
- [Serial Communication - Wikipedia](https://en.wikipedia.org/wiki/Serial_communication)
