# Software SPI

## Usage

```csharp
using (SpiDevice spi = new SoftwareSpi(clk: 18, miso: 23, mosi: 24, cs: 25))
using (Mcp3008 mcp = new Mcp3008(spi))
{
    while (true)
    {
        double value = mcp.Read(0);
        value = value / 10.24;
        value = Math.Round(value);
        Console.WriteLine($"{value}%");
        Thread.Sleep(500);
    }
}
```

It's possible to ignore `cs` by sending in -1 in the parameter (also note that the `SpiConnectionSettings` has a `ChipSelectLine`, either can be used).
You can also use the SoftwareSPI device as an output-only device by sending in -1 for `miso`.

For more advanced usage SpiConnectionSettings can be used.

## Binding notes

Please note that the current SPI implementation does not take into considering any clock frequencies or timing, the data rate is limited by the host performance.
