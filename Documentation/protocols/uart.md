# UART/Serial Communication (RS232/RS485)

UART (Universal Asynchronous Receiver/Transmitter) is a common protocol for serial communication with devices like GPS modules, Bluetooth adapters, GSM modems, and industrial sensors.

## Quick Start: Basic UART Usage

```csharp
using System.IO.Ports;

// Open serial port
SerialPort port = new SerialPort("/dev/ttyS0", 9600, Parity.None, 8, StopBits.One);
port.Open();

// Write data
port.WriteLine("Hello, UART!");

// Read data
string response = port.ReadLine();
Console.WriteLine($"Received: {response}");

// Close port
port.Close();
```

**Note:** Use .NET's `System.IO.Ports.SerialPort` class, not System.Device.* APIs.

## Serial Port Devices on Raspberry Pi

### Available Serial Ports

Raspberry Pi has multiple serial interfaces:

| Port | Device | Default Function | Speed | Recommended Use |
|------|--------|------------------|-------|-----------------|
| UART0 | /dev/ttyAMA0 | Bluetooth (Pi 3+) | Full speed | Internal BT (don't use) |
| UART0 | /dev/serial0 | Symlink to primary | Full speed | General purpose |
| UART1 | /dev/ttyS0 | Console/GPIO | Limited speed | GPIO pins (use this) |
| USB | /dev/ttyUSB0 | USB adapter | Depends on adapter | USB-Serial dongles |

### Typical Configuration

**Raspberry Pi 3, 4, 5:**

- **Primary UART (GPIO 14/15):** `/dev/ttyS0` (mini UART)
- **Bluetooth UART:** `/dev/ttyAMA0` (full UART)
- **Symbolic link:** `/dev/serial0` → primary UART

**Recommendation:** Use `/dev/serial0` for GPIO UART - it automatically points to the correct device.

## Physical Connections

### GPIO UART Pins (Raspberry Pi)

| GPIO Pin | Function | Header Pin | Description |
|----------|----------|------------|-------------|
| GPIO 14 | TXD (TX) | Pin 8 | Transmit data (Raspberry Pi → Device) |
| GPIO 15 | RXD (RX) | Pin 10 | Receive data (Device → Raspberry Pi) |
| Ground | GND | Pin 6, 9, 14, 20, 25, 30, 34, 39 | Common ground |

### Wiring

**Basic UART connection:**

```
Raspberry Pi          Device
GPIO 14 (TX) ────────→ RX
GPIO 15 (RX) ←──────── TX
GND ─────────────────── GND
```

**Important:**

- TX connects to RX (transmit to receive)
- RX connects to TX (receive to transmit)
- Always connect ground

### Voltage Levels

**Critical:** Raspberry Pi GPIO uses **3.3V logic**

- Safe: 3.3V devices (most modern sensors/modules)
- Dangerous: 5V devices (will damage Raspberry Pi!)

**For 5V devices (RS232):**

Use a level shifter like MAX3232:

```
Raspberry Pi (3.3V) ↔ MAX3232 ↔ Device (5V/±12V RS232)
```

## Enabling UART on Raspberry Pi

### Method 1: Using raspi-config (Recommended)

```bash
sudo raspi-config
```

Navigate to:

1. **Interfacing Options** or **Interface Options**
2. **Serial Port**
3. "Would you like a login shell accessible over serial?" → **No**
4. "Would you like the serial port hardware enabled?" → **Yes**
5. Reboot

```bash
sudo reboot
```

### Method 2: Manual Configuration

Edit `/boot/firmware/config.txt`:

```bash
sudo nano /boot/firmware/config.txt
```

> [!Note]
> Prior to *Bookworm*, Raspberry Pi OS stored the boot partition at `/boot/`. Since Bookworm, the boot partition is located at `/boot/firmware/`. Adjust the previous line to be `sudo nano /boot/config.txt` if you have an older OS version.

Add or modify these lines:

```text
# Enable UART
enable_uart=1

# Optional: Disable Bluetooth to use full UART on GPIO 14/15
dtoverlay=disable-bt
```

Disable serial console (frees up UART for your use):

```bash
sudo systemctl disable serial-getty@ttyS0.service
sudo systemctl disable serial-getty@ttyAMA0.service
```

Reboot:

```bash
sudo reboot
```

### Verify UART is Enabled

```bash
ls -l /dev/ttyS0 /dev/serial0
```

Should show both devices exist:

```
crw-rw---- 1 root dialout 4, 64 Jan 10 10:00 /dev/ttyS0
lrwxrwxrwx 1 root root 5 Jan 10 10:00 /dev/serial0 -> ttyS0
```

## Permission Setup

Add your user to the `dialout` group:

```bash
sudo usermod -aG dialout $USER
```

Log out and log back in for changes to take effect.

Verify:

```bash
groups
```

Should include `dialout`.

## Basic Serial Communication

### Opening a Port

```csharp
using System.IO.Ports;

SerialPort port = new SerialPort(
    portName: "/dev/serial0",    // Port name
    baudRate: 9600,              // Baud rate (bits per second)
    parity: Parity.None,         // No parity checking
    dataBits: 8,                 // 8 data bits
    stopBits: StopBits.One);     // 1 stop bit

// Set timeouts
port.ReadTimeout = 1000;  // 1 second
port.WriteTimeout = 1000; // 1 second

port.Open();
```

### Writing Data

```csharp
// Write string (ASCII)
port.Write("AT\r\n");

// Write bytes
byte[] data = { 0x01, 0x02, 0x03 };
port.Write(data, 0, data.Length);

// Write line (adds \n)
port.WriteLine("COMMAND");
```

### Reading Data

```csharp
// Read line (blocks until \n or timeout)
string line = port.ReadLine();

// Read specific number of bytes
byte[] buffer = new byte[10];
int bytesRead = port.Read(buffer, 0, 10);

// Read all available bytes
int available = port.BytesToRead;
if (available > 0)
{
    byte[] data = new byte[available];
    port.Read(data, 0, available);
}

// Read single byte
int byteValue = port.ReadByte();
```

### Closing the Port

```csharp
port.Close();
port.Dispose();
```

Or use `using` for automatic cleanup:

```csharp
using SerialPort port = new SerialPort("/dev/serial0", 9600);
port.Open();
// ... use port
// Automatically closed when exiting 'using' block
```

## Common Baud Rates

| Baud Rate | Use Case | Notes |
|-----------|----------|-------|
| 9600 | Default for many devices | Reliable, widely supported |
| 19200 | Moderate speed | Good for sensors |
| 38400 | Higher speed | Still reliable |
| 57600 | Fast communication | May have errors at distance |
| 115200 | High speed | Common for debug consoles |
| 230400+ | Very fast | Requires good wiring, short cables |

**Raspberry Pi mini UART limitation:** Maximum reliable baud rate is ~250,000 with the mini UART. For higher speeds, swap to the full UART or use USB-Serial adapter.

## Data Format (8N1)

Most devices use **8N1** format:

- **8** data bits
- **N** no parity
- **1** stop bit

Other formats exist but are less common:

```csharp
// 7E1: 7 data bits, even parity, 1 stop bit
SerialPort port = new SerialPort("/dev/serial0", 9600, Parity.Even, 7, StopBits.One);

// 8N2: 8 data bits, no parity, 2 stop bits
SerialPort port = new SerialPort("/dev/serial0", 9600, Parity.None, 8, StopBits.Two);
```

## Flow Control

Flow control manages data transmission speed:

### None (Default)

```csharp
port.Handshake = Handshake.None;
```

Use when:

- Simple point-to-point communication
- Both sides have known, fixed speeds

### Hardware (RTS/CTS)

```csharp
port.Handshake = Handshake.RequestToSend;
```

Requires additional wires:

- RTS (Request To Send)
- CTS (Clear To Send)

### Software (XON/XOFF)

```csharp
port.Handshake = Handshake.XOnXOff;
```

Uses special characters in data stream (can interfere with binary data).

## Example: GPS Module (NMEA)

```csharp
using System.IO.Ports;

using SerialPort gps = new SerialPort("/dev/serial0", 9600);
gps.Open();

Console.WriteLine("Reading GPS data. Press Ctrl+C to exit.");

while (true)
{
    try
    {
        string nmeaSentence = gps.ReadLine();
        
        // Parse NMEA sentence (e.g., $GPGGA)
        if (nmeaSentence.StartsWith("$GPGGA"))
        {
            Console.WriteLine($"GPS: {nmeaSentence}");
            // Parse latitude, longitude, etc.
        }
    }
    catch (TimeoutException)
    {
        // No data received within timeout
        Console.WriteLine("Waiting for GPS data...");
    }
}
```

## Example: AT Command Device (GSM/Bluetooth)

```csharp
using System.IO.Ports;

using SerialPort modem = new SerialPort("/dev/serial0", 9600)
{
    ReadTimeout = 5000,
    NewLine = "\r\n"
};
modem.Open();

// Send AT command
string SendCommand(string command)
{
    modem.WriteLine(command);
    Thread.Sleep(100); // Wait for device to process
    
    StringBuilder response = new StringBuilder();
    while (modem.BytesToRead > 0)
    {
        response.AppendLine(modem.ReadLine());
    }
    
    return response.ToString();
}

// Test connection
Console.WriteLine(SendCommand("AT"));           // Should return "OK"
Console.WriteLine(SendCommand("AT+CGMI"));      // Get manufacturer
Console.WriteLine(SendCommand("AT+CGSN"));      // Get serial number
```

## RS485 Communication

RS485 is differential signaling for long distances and multi-drop networks.

### Hardware Requirements

- RS485 transceiver module (MAX485, MAX3485)
- Direction control (enable TX/RX switching)

### Wiring

```
Raspberry Pi      MAX485        RS485 Bus
GPIO 14 (TX) →    DI            
GPIO 15 (RX) ←    RO            
GPIO 17      →    DE/RE         
                  A    ────────→ A (+ line)
                  B    ────────→ B (- line)
```

### Code Example

```csharp
using System.Device.Gpio;
using System.IO.Ports;

const int directionPin = 17; // DE/RE pin

using GpioController gpio = new();
gpio.OpenPin(directionPin, PinMode.Output);

using SerialPort rs485 = new SerialPort("/dev/serial0", 9600);
rs485.Open();

void SendData(byte[] data)
{
    // Enable transmit mode
    gpio.Write(directionPin, PinValue.High);
    Thread.Sleep(1); // Small delay for transceiver switching
    
    rs485.Write(data, 0, data.Length);
    rs485.BaseStream.Flush(); // Wait for transmission to complete
    
    // Enable receive mode
    gpio.Write(directionPin, PinValue.Low);
}

byte[] ReceiveData(int length)
{
    // Ensure receive mode
    gpio.Write(directionPin, PinValue.Low);
    
    byte[] buffer = new byte[length];
    rs485.Read(buffer, 0, length);
    return buffer;
}
```

## Troubleshooting

### "Port not found" Error

```
System.IO.IOException: The port '/dev/ttyS0' does not exist
```

**Solutions:**

- Verify UART is enabled in `raspi-config`
- Check `/boot/firmware/config.txt` has `enable_uart=1`
- Use `ls /dev/tty*` to see available ports
- Try `/dev/serial0` instead of `/dev/ttyS0`

### "Permission denied" Error

```
System.UnauthorizedAccessException: Access to the path '/dev/ttyS0' is denied
```

**Solutions:**

```bash
sudo usermod -aG dialout $USER
# Log out and back in
```

### No Data Received

**Possible causes:**

1. **Wrong baud rate** - Both sides must match exactly
2. **TX/RX swapped** - Check wiring (TX → RX, RX → TX)
3. **Wrong device** - Try `/dev/serial0`, `/dev/ttyAMA0`, `/dev/ttyUSB0`
4. **Console still active** - Disable `serial-getty` service
5. **Bluetooth using UART** - Add `dtoverlay=disable-bt` to config

### Garbage Characters

**Causes:**

- Baud rate mismatch
- Wrong data format (7 vs 8 bits, parity)
- Electrical noise (use shorter cables, shielding)
- Voltage level mismatch (5V device on 3.3V pin without level shifter)

### Debugging

```bash
# Monitor serial port in terminal
minicom -D /dev/serial0 -b 9600

# Or using screen
screen /dev/serial0 9600

# Send test data with echo
stty -F /dev/serial0 9600
echo "test" > /dev/serial0
cat /dev/serial0
```

## Best Practices

1. **Always use try-catch** for timeout exceptions
2. **Set appropriate timeouts** (1-5 seconds typical)
3. **Close ports when done** - Use `using` statements
4. **Flush buffers** before reading to avoid stale data
5. **Add delays** after commands (devices need processing time)
6. **Verify baud rate** matches device datasheet exactly
7. **Use level shifters** for 5V devices
8. **Keep cables short** (< 15m for RS232, < 1200m for RS485)

## Advanced: Event-Driven Reading

```csharp
using System.IO.Ports;

SerialPort port = new SerialPort("/dev/serial0", 9600);

port.DataReceived += (sender, e) =>
{
    SerialPort sp = (SerialPort)sender;
    string data = sp.ReadExisting();
    Console.WriteLine($"Received: {data}");
};

port.Open();

Console.WriteLine("Listening for data. Press Enter to exit.");
Console.ReadLine();

port.Close();
```

## USB-Serial Adapters

For additional ports or stable full UART:

```bash
# Plug in USB-Serial adapter
# Check for new device
ls /dev/ttyUSB*

# Use in code
SerialPort port = new SerialPort("/dev/ttyUSB0", 9600);
```

**Popular adapters:**

- FTDI FT232 (excellent reliability)
- CH340/CH341 (cheap but may need drivers)
- CP2102 (good balance)

## Next Steps

- [Understanding Protocols](../fundamentals/understanding-protocols.md) - Compare UART with I2C/SPI
- [GPIO Basics](../fundamentals/gpio-basics.md) - Control direction pin for RS485
- [Reading Datasheets](../fundamentals/reading-datasheets.md) - Find UART specs

## Additional Resources

- [UART Protocol - SparkFun](https://learn.sparkfun.com/tutorials/serial-communication)
- [RS232 vs RS485](https://www.omega.com/en-us/resources/rs232-vs-rs485)
- [Raspberry Pi UART Configuration](https://www.raspberrypi.com/documentation/computers/configuration.html#configuring-uarts)
