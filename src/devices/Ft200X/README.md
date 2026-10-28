# FT200X - USB to I2C slave FIFO

The FTDI FT200X (FT200XD) connects a USB host to an **external I2C master**. This binding exposes the USB side through FTDI D2XX, using the same shared infrastructure as the [FT232H](../Ft232H/README.md) and [FT4222](../Ft4222/README.md) bindings.

Internally, `FtCommon` separates the `IFtI2cSlaveTransport` payload FIFO contract from the derived `IFtXSeriesI2cSlaveTransport` contract for persistent FT-X configuration. The shared D2XX implementation can be reused by FT-X I2C slave bindings, including a future FT201X binding; discovery here remains FT200X-specific, and FT201X hardware has not been validated. An FT4222 slave implementation would require LibFT4222 and would not inherit FT-X MTP configuration operations.

**The FT200X cannot act as an I2C master.** It cannot initiate transactions, address a sensor, or provide an `I2cBus` or `I2cDevice` for other bindings. Unlike the MPSSE-capable FT232H, it cannot generate master transfers by sending MPSSE commands. Use an FT232H or FT4222 if you need to drive I2C peripherals directly from a PC.

## How data flows

* `Write` queues payload bytes over USB. The external I2C master must read them from the FT200X at its configured slave address.
* The external master writes bytes to that slave address. `Read` retrieves those bytes over USB.
* `BytesAvailable` reports bytes waiting in the USB driver's receive queue, not free space in the chip or the amount of data waiting for the master.

These are byte FIFOs, not messages or register transactions. I2C START/STOP boundaries are not preserved in the USB byte stream. Agree on framing, lengths, and acknowledgements with your master firmware. Only payload bytes belong in `Write`: do not prepend the slave address or MPSSE commands.

The master supplies SCL, chooses the bus speed, and addresses the FT200X. Opening the binding does not change the slave address, CBUS configuration, or persistent MTP settings, and does not reset or purge the device. Persistent address programming is a separate, explicit operation.

## Requirements and wiring

Install the architecture-appropriate [FTDI D2XX driver/runtime](https://ftdichip.com/drivers/d2xx-drivers/) and make its native library available to the .NET process:

* **Windows:** `ftd2xx.dll`, normally supplied by FTDI's driver installation. Keep the FTDI driver; do not replace it with WinUSB or a HID driver.
* **Linux:** `libftd2xx.so` and USB access permissions for the current user. Follow FTDI's instructions if the VCP driver has claimed the adapter. Do not unload drivers used by other devices indiscriminately.
* **macOS:** `libftd2xx.dylib`. Follow FTDI's instructions concerning competing VCP drivers and USB access.

Match the native library architecture to the running .NET process. No native drivers or vendor binaries are distributed with this binding.

Connect the external master's SDA, SCL, and GND to the FT200X board. Provide pull-ups and power appropriate for both devices' I/O voltages; check the board's schematic rather than assuming USB power implies 3.3 V signaling. Configure the master to use the FT200X's programmed 7-bit slave address.

## Usage

```csharp
using System;
using System.Linq;
using Iot.Device.Ft200X;

var information = Ft200XDevice.GetDevices().Single();
using var device = new Ft200XDevice(
    information, readTimeoutMilliseconds: 1000, writeTimeoutMilliseconds: 1000);

// The external I2C master must read these bytes from the FT200X.
device.Write(new byte[] { 0x01, 0x02, 0x03 });

// Retrieve bytes previously sent by the external I2C master.
byte[] buffer = new byte[16];
int count = device.Read(buffer);
Console.WriteLine(Convert.ToHexString(buffer.AsSpan(0, count)));
```

Discovery filters FT-X descriptors by a USB description starting with `FT200X`. FT200X shares the default USB VID/PID `0403:6015` and the D2XX `FtXSeries` type with other FT-X chips; neither identifies it uniquely. For a customized description, select a descriptor using `FtCommon.GetDevices()` and pass it to the constructor **only after verifying the physical chip**.

Opening uses the serial number, not the Windows-only D2XX location ID. Adapters must have nonempty, unique serial numbers. With multiple adapters, select by serial number instead of enumeration order. Only one application should open the adapter at a time.

`Read` requests up to the buffer length and can return fewer bytes, including zero, after the D2XX timeout. Only the returned bytes are valid. This is not EOF. An empty read or write does nothing. Native D2XX failures throw `IOException`. Missing libraries or incompatible architectures surface the .NET loader error.

`Write` requires D2XX to accept the entire buffer; otherwise it throws `IOException` with the accepted byte count. It never retries a partial write, which could duplicate bytes. Success does not confirm that the master consumed the payload. Without an active master draining the FIFO, sufficiently large writes will time out. Use application-level acknowledgements for delivery.

All instance operations, including disposal, are serialized. An outstanding read may delay a write or disposal until its timeout. Disposing the device releases the native handle and is safe to repeat.

## Programming the I2C address

`ProgrammedI2cAddress` reads the address stored in MTP memory. `ProgramI2cAddress` changes it using D2XX's FT-X EEPROM configuration APIs, without requiring FT_PROG. These APIs are available on Windows, Linux, and macOS. Use a sufficiently recent native D2XX runtime; a missing entry point is surfaced as a loader error, not treated as success.

```csharp
using var device = new Ft200XDevice(information);
Console.WriteLine($"Stored address: 0x{device.ProgrammedI2cAddress:X2}");
bool changed = device.ProgramI2cAddress(0x23);
Console.WriteLine(changed
    ? "Verified. Disconnect/reconnect USB, then open a new device instance."
    : "Already programmed; no MTP write was needed.");
```

Only unshifted, nonreserved 7-bit addresses `0x08` through `0x77` are accepted. Verify that the physical chip is an FT200X, back up its configuration using FT_PROG, stop the external master, and ensure the selected address does not conflict with another device before programming. Maintain stable USB power.

The method reads the current FT-X configuration and descriptor strings, changes only the address, programs once, then reads back and compares **all exposed configuration fields and all four descriptor strings**. USB VID/PID, serial number, power settings, CBUS functions, and other exposed settings are preserved. It does not erase the device or intentionally program the user-data area. This comparison is not a backup or byte-for-byte verification of the entire 2 KB MTP memory.

If the address already matches, the method returns `false`, does not program memory, and leaves the connection open. Once programming is attempted, it always closes the connection, including on write or verification failure. Failures throw; there is no automatic retry or rollback that might consume more programming cycles. Inspect the stored configuration before deciding whether to retry.

After programming, **physically disconnect/reconnect USB and open a new instance** to reload the configuration. The method deliberately does not use Windows-only `FT_CyclePort` or assume that an ordinary D2XX device reset reloads MTP. The stored address getter and a successful readback do not prove that the chip is already responding at the new address. Even a no-op can require a reconnect if an earlier programming operation has not yet been reloaded.

The specified MTP endurance is **2,000 read/write cycles**. Address changes are repeatable, but are intended for occasional configuration, not rapid switching. Do not call programming automatically at sensor startup.

## Sample and tests

The [sample](samples/Program.cs) lists matching devices without opening them:

```shell
dotnet run --project samples/Ft200X.sample.csproj
```

Use the displayed serial number for an explicit operation:

```shell
dotnet run --project samples/Ft200X.sample.csproj -- --status SERIAL
dotnet run --project samples/Ft200X.sample.csproj -- --write SERIAL 01020304
dotnet run --project samples/Ft200X.sample.csproj -- --read SERIAL 4
dotnet run --project samples/Ft200X.sample.csproj -- --get-address SERIAL
```

Address programming requires the explicit `--confirm` flag:

```shell
dotnet run --project samples/Ft200X.sample.csproj -- --program-address SERIAL 0x22 --confirm
```

This command programs real hardware. It is not part of normal discovery, status checks, or sensor startup. After a changed address is verified, follow the reconnect instruction before running the sensor.

`--status` opens the adapter and reports its USB receive queue without consuming or transmitting payload data. `--write` and `--read` require cooperating master firmware; a short sample read reports the actual bytes and returns exit code 1.

### Temperature sensor demo

```shell
dotnet run --project samples/Ft200X.sample.csproj -- --sensor SERIAL
```

This mode mimics a fixed-temperature sensor with three logical 16-bit registers. It is an educational FIFO protocol, **not** an emulation of a standard sensor chip: it cannot service immediate register reads with `WriteRead` or a repeated START. The host PC must receive and process each request over USB before the master starts a separate read.

Before running, back up the FT200X configuration with [FT_PROG](https://ftdichip.com/utilities/) and set its **7-bit I2C slave address to `0x22`**, either with FT_PROG or the explicit `--program-address` command above. Program only the intended adapter and disconnect/reconnect USB to reload the configuration. This is a persistent MTP setting. The `--sensor` mode does not automatically program or verify it. Make sure no other peripheral uses `0x22` on the same bus. Start with empty FIFOs and the master stopped, for example by power-cycling the adapter before starting the sample. The sample does not purge existing data.

| Register | Meaning | Access | Initial value |
|----------|---------|--------|---------------|
| `0x00` | Sensor ID | Read-only | `0x2001` |
| `0x01` | Simulated temperature, signed hundredths of degrees Celsius | Read-only | `2350` = 23.50 C |
| `0x02` | Measurement enabled: `0` disabled, `1` enabled | Read/write | `1` |

The temperature is constant, not a physical measurement. Reading it while disabled returns an explicit error rather than a fabricated temperature.

Each request and response is exactly four bytes. Values are **big-endian**:

* Request: `[command, register, value MSB, value LSB]`. Command `0x00` reads (both value bytes must be zero); `0x01` writes.
* Response: `[status, register, value MSB, value LSB]`. Success is status `0x00` with the read or updated value. On error the value bytes are zero.

| Status | Meaning |
|--------|---------|
| `0x00` | Success |
| `0x01` | Invalid command |
| `0x02` | Unknown register |
| `0x03` | Write to a read-only register |
| `0x04` | Invalid value or nonzero read-request value |
| `0x05` | Temperature measurement disabled |

Examples:

| Operation | Master writes | Master later reads |
|-----------|---------------|--------------------|
| Read sensor ID | `00 00 00 00` | `00 00 20 01` |
| Read temperature | `00 01 00 00` | `00 01 09 2E` |
| Disable measurement | `01 02 00 00` | `00 02 00 00` |
| Read disabled temperature | `00 01 00 00` | `05 01 00 00` |
| Enable measurement | `01 02 00 01` | `00 02 00 01` |

Send **one request at a time** and consume its entire response before sending another. USB read boundaries do not matter: the sample accumulates complete four-byte frames and retains partial requests. There is no automatic resynchronization after dropped bytes or restarting the sample mid-frame. Restart both ends with empty FIFOs after such an error.

The sample-level [FifoSensorRunner](samples/FifoSensorRunner.cs) handles polling, forwarding received bytes, and queuing replies. An [IFifoSensor](samples/IFifoSensor.cs) handler owns request framing and returns a response of any length when ready (or empty memory while accumulating input). Response lengths may depend on the command and need not match request or USB buffer lengths. [TemperatureSensor](samples/TemperatureSensor.cs) implements the existing four-byte protocol; replace that handler to demonstrate another FIFO protocol. This separation does not add I2C transaction boundaries or support immediate register reads. The runner does not own or purge the adapter, and failures propagate without automatic retries.

Here is the corresponding master-side .NET code, using a separate I2C master adapter or a board's native bus:

```csharp
using System;
using System.Device.I2c;
using System.IO;
using System.Threading;

using I2cDevice sensor = I2cDevice.Create(new I2cConnectionSettings(1, 0x22));
sensor.Write(new byte[] { 0x00, 0x01, 0x00, 0x00 }); // Read temperature.
Thread.Sleep(100); // Demonstration delay for USB and the PC sample to process the request.
byte[] reply = new byte[4];
sensor.Read(reply); // Separate transaction, not WriteRead.
if (reply[0] != 0 || reply[1] != 0x01)
{
    throw new IOException($"Unexpected sensor reply: {Convert.ToHexString(reply)}");
}

short hundredthsCelsius = (short)((reply[2] << 8) | reply[3]);
Console.WriteLine($"{hundredthsCelsius / 100.0:F2} C");
```

Bus ID `1` is an example; use your master's bus or an `I2cDevice` from an FT232H/FT4222 bus. The 100 ms delay is illustrative, **not a readiness guarantee**. For initial testing, wait for the sample's `Reply:` message before triggering the master read. A production protocol needs explicit readiness signalling (for example, the chip's configured data-available CBUS signal) or another coordinated handshake. Do not poll by blindly consuming FIFO bytes or assume an empty FIFO behaves like an ordinary sensor register. Press Ctrl+C to stop.

Open `Ft200X.slnx` to work with the binding, sample, and tests. Hardware-independent tests exercise discovery filtering, validation, partial transfers, error propagation, and lifetime:

```shell
dotnet test tests/Ft200X.Tests.csproj
```

For hardware validation on Windows, Linux, and macOS, queue a short known payload and read it from the external master, then write a different payload from the master and verify the exact USB read result. Check short-read timeouts, disconnect errors, reopen, multiple-adapter selection, and backpressure with the master paused. USB-only discovery does not validate I2C communication.

Protocol reference: [FT200XD datasheet](https://ftdichip.com/wp-content/uploads/2025/06/DS_FT200XD.pdf).
