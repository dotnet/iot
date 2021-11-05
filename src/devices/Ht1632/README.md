# Holtek HT1632 - 32×8 & 24×16 LED Driver

The devices are a memory mapping LED display controller/driver, which can select a number of ROW and commons. These are 32 ROW & 8 Commons and 24 ROW & 16 Commons. The devices support 16-gradation LEDs for each out line using PWM control with software instructions. A serial interface is conveniently provided for the command mode and data mode. Only three or four lines are required for the interface between the host controller and the devices. The display can be extended by cascading the devices for wider applications.

## Documentation

- [Datasheet](https://www.holtek.com/documents/10179/116711/HT1632D_32D-2v100.pdf)

## Binding Notes

This binding currently only supports writing commands and raw data with GPIO.

- [X] GPIO
- [ ] SPI

- [X] WRITE Mode
- [ ] READ Mode
- [ ] READ-MODIFY-WRITE Mode

You can find out in the [sample](./samples) how to send images to the device.

## Usage

Initialization

```csharp
using var ht1632 = new Ht1632(new Ht1632PinMapping(cs: 27, wr: 22, data: 17), new GpioController())
{
    ComOption = ComOption.NMos16Com,
    ClockMode = ClockMode.RcMaster,
    Enabled = true,
    PwmDuty = 1,
    Blink = false,
    LedOn = true
};
```

Show image

```csharp
var image = Image.Load<Rgba32>("./dotnet-bot.bmp");
var data = new byte[24 * 16 / 4];

for (var y = 0; y < 24; y++)
{
    for (var x = 0; x < 16; x += 4)
    {
        var index = (x + 16 * y) / 4;
        var value = (byte)(
            (image[x + 0, y].R > 127 ? 0b_1000 : 0) |
            (image[x + 1, y].R > 127 ? 0b_0100 : 0) |
            (image[x + 2, y].R > 127 ? 0b_0010 : 0) |
            (image[x + 3, y].R > 127 ? 0b_0001 : 0));
        data[index] = value;
    }
}

ht1632.WriteData(0, data);
```
