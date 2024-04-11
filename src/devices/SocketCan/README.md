# SocketCan - CAN BUS library (Linux only)

Controller Area Network Protocol Family bindings (SocketCAN).

## Documentation

- SocketCan [documentation](https://www.kernel.org/doc/Documentation/networking/can.txt)

## Usage

### Reading a frame

```csharp
using (CanRaw can = new CanRaw())
{
    byte[] buffer = new byte[8];
    // to scope to specific id
    // can.Filter(id);

    while (true)
    {
        if (can.TryReadFrame(buffer, out int frameLength, out CanId id))
        {
            Span<byte> data = new Span<byte>(buffer, 0, frameLength);
            string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join("", data.ToArray().Select((x) => x.ToString("X2")));
            Console.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
        }
        else
        {
            Console.WriteLine($"Invalid frame received!");
        }
    }
}
```

### Writing a frame

```csharp
CanId id = new CanId()
{
    Standard = 0x1A // arbitrary id
};

using (CanRaw can = new CanRaw())
{
    byte[][] buffers = new byte[][]
    {
        new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 },
        new byte[7] { 1, 2, 3, 40, 50, 60, 70 },
        new byte[0] { },
        new byte[1] { 254 },
    };

    while (true)
    {
        foreach (byte[] buffer in buffers)
        {
            can.WriteFrame(buffer, id);
            string dataAsHex = string.Join("", buffer.Select((x) => x.ToString("X2")));
            Console.WriteLine($"Sending: {dataAsHex}");
            Thread.Sleep(1000);
        }
    }
}
```

## Setup for Raspberry PI and MCP2515

- Connect SPI device to regular SPI pins (SI/MOSI - `BCM 10`; SO/MISO - `BCM 9`; CLK/SCK - `BCM 11`; CS - `CE0`)
- Interrupt pin should be connected to any GPIO pin i.e. `BCM 25` (note: interrupt pin can be adjusted below)
- Add following in `/boot/firmware/config.txt`

> [!Note]
> Prior to *Bookworm*, Raspberry Pi OS stored the boot partition at `/boot/`. Since Bookworm, the boot partition is located at `/boot/firmware/`. Adjust the previous line to be `sudo nano /boot/firmware/config.txt` if you have an older OS version.

```text
dtparam=spi=on
dtoverlay=mcp2515-can0,oscillator=8000000,interrupt=25
dtoverlay=spi-bcm2835-overlay
```

For test run `ifconfig -a` and check if `can0` (or similar) device is on the list.

Now we need to set network bitrate and "start" the network.
Other popular bit rates: 10000, 20000, 50000, 100000, 125000, 250000, 500000, 800000, 1000000

```shell
sudo ip link set can0 up type can bitrate 125000
sudo ifconfig can0 up
```

## Diagnosing the network (tested on Raspberry Pi)

These steps are not required but might be useful for diagnosing potential issues.

- Install can-utils package (i.e. `sudo apt-get install can-utils`)

```shell
sudo apt-get -y install can-utils
```

- On first device listen to CAN frames (can be also sent on the same device but ensure separate terminal)

```shell
candump can0
```

- On second device send a packet

```shell
cansend can0 01a#11223344AABBCCDD
```

- On the first device you should see the packet being send by the second device
