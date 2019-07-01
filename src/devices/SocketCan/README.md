# SocketCan - CAN BUS library (Linux only)

Controller Area Network Protocol Family bindings (SocketCAN).

[samples/README.md](See sample for usage.)

## Setup for Raspberry PI and MCP2515

- Connect SPI device to regular SPI pins (SI/MOSI - `BCM 10`; SO/MISO - `BCM 9`; CLK/SCK - `BCM 11`; CS - `CE0`)
- Interrupt pin should be connected to any GPIO pin i.e. `BCM 25` (note: interrupt pin can be adjusted below)
- Add following in `/boot/config.txt`

```
dtparam=spi=on
dtoverlay=mcp2515-can0,oscillator=8000000,interrupt=25
dtoverlay=spi-bcm2835-overlay
```

For test run `ifconfig -a` and check if `can0` (or similar) device is on the list.

Now we need to set network bitrate and "start" the network.
Other popular bit rates: 10000, 20000, 50000, 100000, 125000, 250000, 500000, 800000, 1000000

```sh
sudo ip link set can0 up type can bitrate 125000
sudo ifconfig can0 up
```

## Diagnosing the network (tested on Raspberry Pi)

These steps are not required but might be useful for diagnosing potential issues.

- Install can-utils package (i.e. `sudo apt-get install can-utils`)

```sh
sudo apt-get -y install can-utils
```

- On first device listen to CAN frames (can be also sent on the same device but ensure seperate terminal)

```sh
candump can0
```

- On second device send a packet

```sh
cansend can0 01a#11223344AABBCCDD
```

- On the first device you should see the packet being send by the second device


## References

- https://www.kernel.org/doc/Documentation/networking/can.txt
