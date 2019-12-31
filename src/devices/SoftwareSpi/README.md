# Software SPI

Usage is very simple:

```csharp
using (SpiDevice spi = new SoftwareSpi(clk: 6, miso: 23, mosi: 5, cs: 24))
{
   // do stuff over SPI
}
```

For more advanced usage it optionally also takes GpioController and SpiConnectionSettings.
