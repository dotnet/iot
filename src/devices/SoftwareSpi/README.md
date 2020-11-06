# Software SPI

Usage is very simple:

```csharp
using (SpiDevice spi = new SoftwareSpi(clk: 6, miso: 23, mosi: 5, cs: 24))
{
   // do stuff over SPI
}
```

It's possible to ignore `cs` by sending in -1 in the parameter (also note that the `SpiConnectionSettings` has a `ChipSelectLine`, either can be used).
You can also use the SoftwareSPI device as an output-only device by sending in -1 for `miso`.

Please note that the current SPI implementation does not take into considering any clock frequencies or timing, the data rate is limited by the host performance.

For more advanced usage it optionally also takes GpioController and SpiConnectionSettings.
