# Digital liquid level switch

Digital liquid level switches are devices that can detect the presence of liquid/water. GPIO can be used to communicate with the devices.

## Documentation

The implementation supports any single pin output digital liquid level switch.

- LLC200D3SH sensor [datasheet](https://cdn-shop.adafruit.com/product-files/3397/3397_datasheet_actual.pdf)

## Usage

Define the LLC200D3SH sensor using the LiquidLevelSwitch class.

```c#
using (LiquidLevelSwitch sensor = new LiquidLevelSwitch(23, PinValue.Low))
{
    while (true)
    {
        // read liquid level switch
        Console.WriteLine($"Detected: {sensor.IsLiquidPresent()}");
        Console.WriteLine();

        Thread.Sleep(1000);
    }
}
```

An example on how to use the specific LLC200D3SH device binding is available in the [samples](samples) folder.

## LLC200D3SH Circuit

The following fritzing diagram illustrates one way to wire up the Optomax LLC200D3SH digital liquid level switch with a Raspberry Pi.

![Raspberry Pi Breadboard diagram](rpi-llc200d3sh_bb.png)
