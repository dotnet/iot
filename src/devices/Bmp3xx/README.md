# BMP390/BMP388 - barometric pressure, altitude and temperature sensor

The Bosch BMP390 and BMP388 are digital barometric pressure and temperature sensors, the successors of the BMP280. The BMP390 is the more accurate of the two (relative accuracy typically ±3 Pa, about ±25 cm of altitude). Both use the same registers and differ only in their chip ID, so this binding supports both with the `Bmp390` and `Bmp388` classes. They are sold on breakout boards such as the Adafruit BMP390 (product 4816) and the Adafruit BMP388, and on many generic boards.

## Documentation

- BMP390 [datasheet](https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmp390-ds002.pdf)
- BMP388 [datasheet](https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmp388-ds001.pdf)

## Usage

```csharp
// I2C bus 1 on the Raspberry Pi; use Bmp3xxBase.SecondaryI2cAddress (0x76) if the SDO pin is low.
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bmp3xxBase.DefaultI2cAddress));
using var sensor = new Bmp390(i2cDevice); // or new Bmp388(i2cDevice)

sensor.PressureSampling = Bmp3xxOversampling.X8;
sensor.TemperatureSampling = Bmp3xxOversampling.X1;
sensor.FilterCoefficient = Bmp3xxFilterCoefficient.Coefficient3;

// Starts one measurement, waits for it and returns the result.
Bmp3xxReadResult result = sensor.Read();
Console.WriteLine($"Temperature: {result.Temperature?.DegreesCelsius:0.00} °C");
Console.WriteLine($"Pressure: {result.Pressure?.Hectopascals:0.00} hPa");

// The altitude needs the current sea-level pressure; without it, the standard 1013.25 hPa is assumed.
if (sensor.TryReadAltitude(out Length altitude))
{
    Console.WriteLine($"Altitude: {altitude.Meters:0.0} m");
}
```

To measure continuously, set `OutputDataRate` and switch to normal mode with `SetPowerMode(Bmp3xxPowerMode.Normal)`; `TryReadTemperature`, `TryReadPressure` and `TryReadAltitude` then return the latest values. The [sample](samples/Program.cs) shows both modes.

## Wiring

For I2C, connect the board to the Raspberry Pi like this, with the Pi switched off:

| BMP390/BMP388 board | Raspberry Pi |
|---------------------|--------------|
| VIN (or VCC)        | 3.3V (pin 1) |
| GND                 | GND (pin 6)  |
| SCK                 | SCL (GPIO 3, pin 5) |
| SDI                 | SDA (GPIO 2, pin 3) |
| SDO                 | GND for address 0x76, 3.3V for address 0x77 |
| CS                  | not connected, or 3.3V (I2C mode) |

Many breakout boards connect SDO and CS already; check your board's documentation. If CS is pulled low once, the sensor switches to SPI until it is powered off (BMP390 datasheet, section 5.1).

## Binding Notes

Implemented:

- I2C
- Temperature, pressure and altitude: `Read`, `ReadAsync`, `TryReadTemperature`, `TryReadPressure`, `TryReadAltitude`
- Pressure and temperature oversampling, IIR filter coefficient, output data rate, power mode (sleep, forced, normal)
- Status and error flags (`ReadStatus`, `ReadErrors`), soft reset (`Reset`)

Not implemented: SPI, FIFO, interrupt pin (data-ready, FIFO watermark and FIFO full interrupts), I2C watchdog, sensor time, self-test.

Behavior to be aware of:

- `Read` and `ReadAsync` start a measurement unless the sensor is in normal mode, and return only that measurement: data left from an earlier measurement that wasn't read is discarded. Calling `SetPowerMode(Bmp3xxPowerMode.Forced)` first isn't needed, and makes `Read` wait for that measurement to finish before starting its own. `TryReadTemperature`, `TryReadPressure` and `TryReadAltitude` never start one: they return the latest values, or `false` if nothing has been measured since the last reset.
- Values outside the sensor's operating range (-40 to 85 °C, 300 to 1250 hPa) are not reported: `Read` returns `null` for that value and the `TryRead` methods return `false`. The pressure is computed from the temperature, so an out-of-range temperature makes the pressure unavailable too.
- If a measurement doesn't finish in time, `Read` returns `null` for both values instead of throwing.
- `SetPowerMode(Bmp3xxPowerMode.Normal)` throws an `InvalidOperationException` if the output data rate is too fast for the oversampling settings; the sensor stays in sleep mode. `GetMeasurementDuration` tells how long one measurement takes.
- Leaving normal or forced mode with `SetPowerMode` waits for the running measurement to finish (about 6 ms with the default settings, up to about 145 ms with x32 oversampling for both), because the sensor ignores a new mode until then.
