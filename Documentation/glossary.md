# Glossary of IoT and Electronics Terms

This glossary explains common terms used in .NET IoT development and hardware interfacing.

## General Terms

### ADC (Analog-to-Digital Converter)

Converts analog voltages (0-3.3V) to digital values (0-1023, 0-4095, etc.). Raspberry Pi GPIO pins are **digital only** - use external ADC for analog sensors.

### BCM Numbering

Broadcom chip GPIO numbering (GPIO0, GPIO1, etc.). This is what .NET IoT uses. Different from physical pin numbers.

### Breakout Board

A small PCB that makes a chip or sensor easier to use by exposing pins in a breadboard-friendly format.

### Breadboard

A prototyping board with holes for inserting component leads and wires without soldering.

### GPIO (General-Purpose Input/Output)

Programmable pins that can be set as input or output for digital signals.

### HAT (Hardware Attached on Top)

An add-on board designed for Raspberry Pi with a 40-pin connector. Follows specific mechanical and electrical specifications.

### IoT (Internet of Things)

Network of physical devices embedded with sensors, software, and connectivity to exchange data.

### MCU/SBC (Microcontroller/Single-Board Computer)

- **MCU:** Microcontroller (Arduino, ESP32) - single-purpose, real-time, bare-metal
- **SBC:** Single-board computer (Raspberry Pi) - runs full OS, multi-purpose

### SoC (System on Chip)

A complete computer system on a single chip (CPU, GPU, RAM, I/O). Example: BCM2711 on Raspberry Pi 4.

## Electrical Terms

### Active High / Active Low

- **Active High:** Signal is TRUE when HIGH (3.3V), FALSE when LOW (0V)
- **Active Low:** Signal is TRUE when LOW (0V), FALSE when HIGH (3.3V)

Example: Button with pull-up resistor is active-low (pressed = LOW).

### Current (Amperage)

Flow of electric charge, measured in Amperes (A) or milliamperes (mA). Raspberry Pi GPIO pins can source/sink ~16mA safely.

### Decoupling Capacitor

Capacitor placed near power pins to filter noise and stabilize voltage. Typically 0.1µF.

### Flyback Diode

Diode placed across inductive loads (motors, relays) to protect against voltage spikes.

### Ground (GND)

Reference voltage point (0V). All devices must share a common ground.

### Logic Level

Voltage range representing digital HIGH and LOW:

- **3.3V logic:** HIGH = ~3.3V, LOW = ~0V (Raspberry Pi)
- **5V logic:** HIGH = ~5V, LOW = ~0V (Arduino, some sensors)

**Warning:** Don't connect 5V signals directly to Raspberry Pi GPIO!

### Open Drain / Open Collector

Output that can pull to LOW but not drive HIGH (requires pull-up resistor). Used in I2C.

### Pull-up / Pull-down Resistor

Resistor that sets default voltage level:

- **Pull-up:** Connects pin to 3.3V (default HIGH)
- **Pull-down:** Connects pin to GND (default LOW)

Prevents floating inputs. Typical values: 4.7kΩ - 10kΩ.

### Voltage

Electric potential difference, measured in Volts (V). Raspberry Pi GPIO operates at 3.3V.

### Voltage Divider

Circuit using two resistors to reduce voltage. Used to interface 5V sensors with 3.3V GPIO.

## Communication Protocols

### I2C (Inter-Integrated Circuit)

Two-wire serial protocol (SDA, SCL) for communicating with sensors/peripherals. Supports multiple devices on same bus using addresses.

- **Speed:** 100 kHz (standard), 400 kHz (fast)
- **Wires:** 2 (SDA, SCL) + ground
- **Devices:** Up to 128 addresses

### SPI (Serial Peripheral Interface)

Four-wire protocol for high-speed communication with displays, SD cards, etc.

- **Speed:** 10+ MHz
- **Wires:** 4+ (MOSI, MISO, SCK, CS)
- **Devices:** Limited by CS pins

### UART (Universal Asynchronous Receiver/Transmitter)

Serial communication protocol (TX, RX) for point-to-point connections.

- **Speed:** 9600 - 115200 baud (common)
- **Wires:** 2 (TX, RX) + ground
- **Devices:** One per port

### PWM (Pulse Width Modulation)

Technique for creating analog-like output by rapidly toggling digital pin. Used for LED dimming, motor speed control.

- **Frequency:** How many pulses per second (Hz)
- **Duty Cycle:** Percentage of time signal is HIGH (0-100%)

### 1-Wire

Single-wire protocol for temperature sensors (DS18B20) and authentication.

## GPIO Terms

### Chip Select (CS)

SPI signal to select which device on the bus is active. Also called SS (Slave Select).

### Clock (SCK/SCL)

Signal that synchronizes data transmission. Master device generates clock.

### Debouncing

Filtering out noise/bounces from mechanical switches to register single clean press.

### Duty Cycle

In PWM, percentage of time signal is HIGH. 0% = always LOW, 100% = always HIGH, 50% = half brightness.

### Edge

Transition in signal:

- **Rising Edge:** LOW → HIGH
- **Falling Edge:** HIGH → LOW

### Frequency

Number of cycles per second, measured in Hertz (Hz). For PWM: how many times signal toggles per second.

### Interrupt

Hardware signal that triggers immediate attention from CPU. More efficient than polling.

### MISO/MOSI (Master In Slave Out / Master Out Slave In)

SPI data lines:

- **MOSI:** Data from master to slave
- **MISO:** Data from slave to master

### Pin Mode

Configuration of GPIO pin:

- **Input:** Read external signal
- **Output:** Drive signal HIGH or LOW
- **InputPullUp:** Input with internal pull-up resistor
- **InputPullDown:** Input with internal pull-down resistor

### Polling

Repeatedly checking input state in a loop. Less efficient than interrupts.

### RX/TX (Receive/Transmit)

UART data lines:

- **TX:** Transmit data out
- **RX:** Receive data in

**Wiring:** Connect TX of one device to RX of other.

## Device Terms

### Baud Rate

Speed of serial communication in bits per second. Common: 9600, 115200.

### Data Sheet

Technical document describing device specifications, pin functions, and usage.

### Register

Memory location in a device that controls settings or holds data. Accessed via I2C/SPI.

### Slave Address

7-bit or 8-bit identifier for I2C device (e.g., 0x76). Each device on bus needs unique address.

## Software Terms

### Device Binding

.NET library that wraps low-level protocol communication for specific device. Example: `Iot.Device.Bindings` package contains 130+ bindings.

### Driver

Software that interfaces with hardware. Examples:

- **LibGpiodDriver:** Modern Linux GPIO driver
- **SysFsDriver:** Legacy Linux GPIO driver

### Firmware

Low-level software embedded in hardware devices.

### Library

Collection of reusable code. **System.Device.Gpio** is core .NET IoT library.

## Linux-Specific Terms

### Device File

Special file in `/dev/` that represents hardware. Examples: `/dev/i2c-1`, `/dev/spidev0.0`, `/dev/gpiochip0`.

### Device Tree

Linux kernel configuration describing hardware. Loaded at boot, configured in `/boot/firmware/config.txt`.

### libgpiod

Modern Linux library for GPIO access using character device interface.

### sysfs

Legacy Linux filesystem interface for hardware in `/sys/class/`. Being deprecated for GPIO.

### udev

Linux subsystem for device management and permissions. Rules in `/etc/udev/rules.d/`.

## Raspberry Pi Terms

### dtoverlay (Device Tree Overlay)

Configuration in `/boot/firmware/config.txt` to enable hardware features (I2C, SPI, PWM).

Example: `dtoverlay=spi0-1cs`

### dtparam (Device Tree Parameter)

Configuration parameter for device tree. Example: `dtparam=i2c_arm=on`

### GPIO Chip

Linux representation of GPIO hardware. Raspberry Pi 1-4 use `gpiochip0`, Pi 5 uses `gpiochip4`.

### Physical Pin

Pin position on 40-pin header (1-40). **Don't use in code** - use GPIO numbers instead.

### raspi-config

Configuration tool for Raspberry Pi. Used to enable I2C, SPI, UART, etc.

## Units and Abbreviations

| Term | Meaning | Example |
|------|---------|---------|
| mA | Milliampere (1/1000 A) | GPIO max current: 16mA |
| mV | Millivolt (1/1000 V) | Logic level: 2800mV |
| µA | Microampere (1/1,000,000 A) | Sleep current: 10µA |
| kHz | Kilohertz (1000 Hz) | I2C speed: 400kHz |
| MHz | Megahertz (1,000,000 Hz) | SPI speed: 10MHz |
| kΩ | Kilohm (1000 Ω) | Pull-up: 4.7kΩ |
| µF | Microfarad (1/1,000,000 F) | Capacitor: 0.1µF |
| pF | Picofarad (1/1,000,000,000,000 F) | Capacitor: 22pF |

## Acronyms

| Acronym | Full Name |
|---------|-----------|
| ADC | Analog-to-Digital Converter |
| BCM | Broadcom |
| CS | Chip Select |
| DAC | Digital-to-Analog Converter |
| GPIO | General-Purpose Input/Output |
| GND | Ground |
| HAT | Hardware Attached on Top |
| I2C | Inter-Integrated Circuit |
| IoT | Internet of Things |
| LED | Light-Emitting Diode |
| LSB | Least Significant Bit |
| MCU | Microcontroller Unit |
| MISO | Master In Slave Out |
| MOSI | Master Out Slave In |
| MSB | Most Significant Bit |
| PCB | Printed Circuit Board |
| PWM | Pulse Width Modulation |
| RTC | Real-Time Clock |
| RX | Receive |
| SBC | Single-Board Computer |
| SCK | Serial Clock |
| SCL | Serial Clock Line |
| SDA | Serial Data Line |
| SoC | System on Chip |
| SPI | Serial Peripheral Interface |
| SS | Slave Select |
| TX | Transmit |
| UART | Universal Asynchronous Receiver/Transmitter |
| USB | Universal Serial Bus |
| VCC | Voltage Common Collector (power) |

## Common Confusions

### GPIO Number vs Physical Pin

- **GPIO 18** ≠ **Pin 18**
- GPIO 18 = Physical Pin 12
- Always use GPIO numbers in code

### 7-bit vs 8-bit I2C Addresses

- **7-bit** (most common): 0x76
- **8-bit** (rare): 0xEC (write), 0xED (read)
- .NET IoT uses 7-bit addresses

### HIGH/LOW vs 1/0 vs True/False

All equivalent in digital logic:

- HIGH = 1 = True = 3.3V
- LOW = 0 = False = 0V

### Voltage vs Current

- **Voltage:** Like water pressure (3.3V = potential)
- **Current:** Like water flow (16mA = actual flow)
- Exceeding voltage damages components instantly
- Exceeding current damages over time (heat)

## See Also

- [GPIO Basics](fundamentals/gpio-basics.md)
- [Understanding Protocols](fundamentals/understanding-protocols.md)
- [Reading Datasheets](fundamentals/reading-datasheets.md)
- [Troubleshooting](troubleshooting.md)
