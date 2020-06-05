# Digital liquid level switch

Digital liquid level switches are devices that can detect the presence of liquid/water. GPIO can be used to communicate with the devices.

## Device Family

The implementation supports any single pin output digital liquid level switch.

## Usage
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
