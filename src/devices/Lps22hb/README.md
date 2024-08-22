# LPS22HB - Piezoresistive pressure and thermometer sensor

Some of the applications mentioned by the datasheet:

- Altimeter and barometer for portable devices
- GPS applications
- Weather station equipment
- Sport watches

## References

- [LPS22HB datasheet](https://www.st.com/resource/en/datasheet/lps22hb.pdf)

## How to use

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Lps22hb.DefaultI2cAddress);

I2cDevice device = I2cDevice.Create(settings);

using Lps22hb sensor = new Lps22hb(device);

while (true)
{
    sensor.TryReadPressure(out var pressure);
    sensor.TryReadTemperature(out var temperature);

    Console.WriteLine($"Pressure: {pressure.Hectopascals:0.##}hPa");
    Console.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");

    Thread.Sleep(1000);
}
```

## Features bindings

- [x] Interface I2c
- [x] Read pressure (24 bit)
- [x] Read temperature (16 bit)
- [x] Configurable: Continus mode with rate (1Hz,10Hz,25Hz,50Hz,75Hz)
- [x] Configurable: BDU (Block Data Update) configurable
- [x] Configurable: Low-pass filter (Lps22hb embeds an additional low-pass filter that can be applied)
- [x] Device WHO_AM_I
- [x] Device software reset

- [ ] Interface SPI
- [ ] Low power mode
- [ ] One shot mode
- [ ] FIFO mode
- [ ] Status
