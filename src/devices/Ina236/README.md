# INA219 - Bidirectional Current/Power Monitor

The INA236 is a current shunt and power monitor with an I2C-compatible interface. It is an improved version of the INA219 with a higher accuracy and an extra voltage sensor for the secondary side. The device monitors both shunt voltage drop and bus supply voltage, with programmable conversion times and filtering. A programmable calibration value, combined with an internal multiplier, enables direct readouts of current in amperes. An additional multiplying register calculates power in watts.

* Senses Bus Voltages from 0 to 26 V
* Reports Current, Voltage, and Power
* 16 Programmable Addresses
* High Accuracy: 0.5% (Maximum) Over Temperature
* Filtering Options
* Calibration Registers
* Two variants available: Address range 0x40-0x43 or 0x60-0x63

## Documentation

* [INA236 Datasheet](http://www.ti.com/lit/ds/symlink/ina236.pdf)

## Usage

```csharp
const byte Adafruit_Ina236_I2cAddress = 0x40;

// create an INA236 device on I2C bus 1 addressing channel 64
// Known breakouts often have a shunt resistor of 0.008 Ohms and are designed to measure up to 10 Amperes.
using Ina219 device = new Ina236(new I2cConnectionSettings(Adafruit_Ina236_I2cBus, Adafruit_Ina219_I2cAddress),
    ElectricResistance.FromMilliohms(8), ElectricCurrent.FromAmperes(10.0));

Console.WriteLine("Device initialized. Default settings used:");
Console.WriteLine($"Operating Mode: {device.OperatingMode}");
Console.WriteLine($"Number of Samples to average: {device.AverageOverNoSamples}");
Console.WriteLine($"Bus conversion time: {device.BusConversionTime}us");
Console.WriteLine($"Shunt conversion time: {device.ShuntConversionTime}us");

while (!Console.KeyAvailable)
{
    // write out the current values from the INA219 device.
    Console.WriteLine($"Bus Voltage {device.ReadBusVoltage()} Shunt Voltage {device.ReadShuntVoltage().Millivolts}mV Current {device.ReadCurrent()} Power {device.ReadPower()}");
    Thread.Sleep(1000);
}

```

### Notes

To set up the binding, the shunt resistor value and the maximum expected current need to be provided. Known breakout boards
(e.g. from Adafruit or Joy-It) have a shunt resistor of 0.008 Ohms. With a 10 A load, the voltage drop at the resistor is thus
0.08 V, resulting in a power dissipation of 0.8 Watts. 

