# CAP1208 - 8-channel capacitive touch sensor

The CAP1208 from Microchip detects touches on 8 capacitive sensor inputs (CS1 to CS8) and communicates over I2C. It is used, for example, for the 8 touch pads of the Pimoroni Explorer HAT Pro.

## Documentation

* CAP1208 [datasheet](https://ww1.microchip.com/downloads/en/DeviceDoc/00001570C.pdf)

## Usage

```csharp
I2cConnectionSettings settings = new(busId: 1, deviceAddress: Cap1208.DefaultI2cAddress);
using Cap1208 cap1208 = new(I2cDevice.Create(settings));

// Read several touches at the same time (the chip blocks them by default)
cap1208.MultipleTouchBlocking = false;

while (true)
{
    SensorInputs touched = cap1208.ReadTouchedInputs();
    if (touched.HasFlag(SensorInputs.Input1))
    {
        Console.WriteLine("Input 1 touched");
    }

    Thread.Sleep(100);
}
```

* `ReadTouchedInputs` returns the inputs touched since the previous call, including the inputs that are still touched. The chip keeps a touch in its status register until it is read, so a short touch between two calls is not lost.
* The constructor checks the product and manufacturer IDs, and does not change the configuration of the chip.
* `MultipleTouchBlocking`: with the power-on configuration, the chip reports one touch at a time and blocks the other inputs. Set it to `false` to read several touches at the same time.
* `Sensitivity`: from `Multiplier1` (least sensitive) to `Multiplier128` (most sensitive). The power-on default is `Multiplier32`.
* `Recalibrate` forces a new calibration, for example when the surroundings of the sensor pads change.

The binding reads the touches by polling: it does not use the ALERT# pin of the chip.

### Pimoroni Explorer HAT Pro

The touch pads of the Explorer HAT Pro are numbered in a different order than the inputs of the chip:

| Pad | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|---|---|---|---|---|---|---|---|
| Input | `Input5` | `Input6` | `Input7` | `Input8` | `Input1` | `Input2` | `Input3` | `Input4` |
