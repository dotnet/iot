# DFRobot KIT0176: I2C 1kg Weight Sensor Kit - HX711 (Gravity: I2CWeight Sensor)

Binding for I2C interface for HX711 provided by DFRobot.
Note that this binding does not provide way to communicate with HX711 directly.

## Documentation

There is no official datasheet as of writing this document. Some resources with partial information are available though:

- [DFRobot HX711 Weight Sensor Kit](https://wiki.dfrobot.com/HX711_Weight_Sensor_Kit_SKU_KIT0176)
- [C++/Python implementation](https://github.com/DFRobot/DFRobot_HX711_I2C)
- [Product website](https://www.dfrobot.com/product-2289.html)

## How to connect

| Name              | PCB description |
| ----------------- | --------------- |
| Sensor red wire   | E+              |
| Sensor black wire | E-              |
| Sensor white wire | S-              |
| Sensor green wire | S+              |

## Usage

This specific sample uses FT4222 for I2C:

```csharp
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Ft4222;
using Iot.Device.Hx711;

List<Ft4222Device> ft4222s = Ft4222Device.GetFt4222();
if (ft4222s.Count == 0)
{
    Console.WriteLine("FT4222 not plugged in");
    return;
}

Ft4222Device ft4222 = ft4222s[0];

// If using Raspberry Pi rather than FT4222 following initialization method can be used instead:
// using Hx711I2c hx711 = new(I2cDevice.Create(new I2cConnectionSettings(1, Hx711I2c.DefaultI2cAddress)));
using Hx711I2c hx711 = new(ft4222.CreateI2cDevice(new I2cConnectionSettings(0, Hx711I2c.DefaultI2cAddress)));
hx711.CalibrationScale = 2236.0f;
hx711.Tare(blinkLed: true);

// less accuracy but faster response time
hx711.SampleAveraging = 10;

// To simulate pressing CAL button:
// hx711.StartCalibration();
while (true)
{
    Console.WriteLine($"{hx711.GetWeight().Grams:0.0}g");
    Thread.Sleep(1000);
}
```

## Calibration

- Calibration starts when CAL button is pressed or `hx711.StartCalibration()` is called
- When calibration starts orange LED will turn on
- After that wait a bit (min 1 second up to 5 seconds) and place calibration weight (100g by default)
- When weight is placed the calibration will be triggered automatically after around 1 second after placing the weight
- When calibration is successful orange LED will blink 3 times
- When calibration is not succesful orange LED will just turn off without blinking
- Calibration weight can be specified using `AutomaticCalibrationWeight`
- Weight triggering calibration (after CAL button is pressed) can be set using `AutomaticCalibrationThreshold` and is 10g by default
- `AutomaticCalibrationThreshold` should be smaller than `AutomaticCalibrationWeight`

```csharp
hx711.StartCalibration();

// when orange LED blinks 3 times this value can be read and assigned:
hx711.CalibrationScale = GetAutomaticCalibrationScale();

// The assignment is optional - it will also happen automatically when hx711.GetWeight() is called
```

## Auto-calibration

[See official documentation how to use CAL button](https://wiki.dfrobot.com/HX711_Weight_Sensor_Kit_SKU_KIT0176#target_9)
