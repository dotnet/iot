# Ina226 - Bidirectional Current/Power Monitor w/ Alert

## Summary

The INA226 is a current shunt and power monitor with an I2C or SMBUS-compatible
interface. The device monitors both a shunt voltage drop and bus supply voltage.
Programmable calibration value, conversion times, and averaging, combined with
an internal multiplier, enable direct readouts of current in amperes and power
in watts.

* Senses Bus Voltages from 0 to 36 V
* High-Side or Low-Side Sensing
* Reports Current, Voltage, and power
* High Accuracy: 0.1% Gain Error (Max) 10uv Offset(Max)
* Configurable Averaging Options
* 16 Programmable Addresses

## Documentation

* [INA226 Datasheet](https://www.ti.com/lit/ds/symlink/ina226.pdf)

## Potential Smoke Warning

INA226 has a limited common-mode voltage range. This means that if you want to measure, for example solar panel voltage, then you will need a common negative ground charge controller and have the ina226 and shunt connected with a contiguous ground between them and the device connected to the ina226 through i2c. This can be avoided by using an i2c isolator AND a power isolator. There are multiple options for this but one example would be the ADM3260 or similar by other manufacturers.

Also, this board has been modified to add a filter circuit, do not assume that a pre-purchased board will have the same current capability as the one used with the creation of this binding.  Most of them come with .01 ohm resistors which have a small current carrying capability so it has been replaced with a larger shunt of a lower resistance.

## To-Do List

* Needs modification to allow for negative current capability (will get to this soon).
* Fritzing Diagram
* SMBUS Alert Response (feature has not been implemented)
* Alert pin related telemetry for the three different alert flags still needs to be tested.
* Input validation on properties to prevent out of range exceptions

## Binding Notes

This sample uses a generic INA226 breakout board and is hooked to a generic 100 amp / 75 mv shunt with a .1uf ceramic capacitor across In+/In- and two ten ohm resistors between shunt and In+/In- as a filter circuit. The example prints the different registers and configuration every second until you press ESC. The Alert pin is configured for Over-Voltage fault in this example, meaning that if the bus voltage exceeds the configured bus voltage limit, the alert pin will be triggered.  Configuration allows for a triggered high or triggered low condition and in the example the polarity has been set opposite from default for demonstration purposes.

The calibration is determined by the current least significant bit formula found in the datasheet.  Provide the shunt resistance and the expected max current and SetCalibration() will do the rest.

When the INA226 receives the Reset command, it completely resets registers to default and therefore you need to reconfigure anytime you send a Reset command as well.

```csharp
using Iot.Device.Ina226;

// Default i2c address for the INA226
const byte INA226_I2cAddress = 0x40;

// Default i2c bus on raspberry pi
const byte INA226_I2cBus = 0x1;

using (Ina226 ina226 = new(new System.Device.I2c.I2cConnectionSettings(INA226_I2cBus, INA226_I2cAddress)))
{
    // reset device (resets all values to default)
    ina226.Reset();

    // Set number of samples to average in this example to 4
    ina226.SamplesAveraged = Ina226SamplesAveraged.Quantity_4;

    // Set shunt conversion time of a sample to 2,116 microseconds
    ina226.ShuntConvTime = Ina226ShuntConvTime.Time2116us;

    // Set bus voltage conversion time of a sample to 2,116 microseconds
    ina226.BusConvTime = Ina226BusVoltageConvTime.Time2116us;

    // Set the alert pin mode (When shunt voltage over the trigger limit value then trigger pin will by default be pulled low unless alert polarity has been set to true)
    ina226.AlertMode = Ina226AlertMode.ShuntOverVoltage;

    // Set the alert trigger limit value (14.25 volts in this case)
    ina226.AlertLimitVoltage = UnitsNet.ElectricPotential.FromVolts(14.25);

    // Setting the alert polarity to trigger high instead of default trigger low. (false)
    ina226.AlertPolarity = true;

    // Set Calibration
    ina226.SetCalibration(0.00075, 109.226); // 109.226 being the max current in amps at the max mv reading capability of the INA226, so 100 amp * (81.92/75)


    do
    {
        while (! System.Console.KeyAvailable)
        {
            System.Console.Clear();
            System.Console.WriteLine("Press ESC to stop");
            System.Console.WriteLine();

            // Read and print to console the values stored in the INA226 Registers
            System.Console.WriteLine($"Bus Voltage: {ina226.ReadBusVoltage().Volts}v");
            System.Console.WriteLine($"Shunt Voltage: {ina226.ReadShuntVoltage().Millivolts}mV");
            System.Console.WriteLine($"Current: {ina226.ReadCurrent().Amperes} amps");
            System.Console.WriteLine($"Power: {ina226.ReadPower().Watts} watts");
            System.Console.WriteLine($"Alert Mode: {ina226.AlertMode}");
            System.Console.WriteLine($"Alert Trigger Value: {ina226.AlertLimitVoltage.Volts}v");
            System.Console.WriteLine($"Alert Latch Enabled Flag:{ina226.AlertLatchEnable}");
            System.Console.WriteLine($"Alert Polarity: {ina226.AlertPolarity}");

            System.Threading.Thread.Sleep(1000);
        }
    }
    while (System.Console.ReadKey(true).Key != System.ConsoleKey.Escape);
}

```

## Configuration Notes

The INA226 allows for specifying the number of samples to average as well as the conversion time for both shunt voltage measurement and bus voltage measurements. Further information on the configuration values available and other information can be found in the sample project readme.

| Samples To Average | Sample Conversion Time | Alert Operating Modes |
| :---               | :---                   | :--- |
| 1 | 140us | Shunt Over Voltage |
| 4 | 204us | Shunt Under Voltage |
| 16 | 332us | Bus Over Voltage |
| 64 | 588us | Bus Under Voltage |
| 128 | 1100us | Power Over Limit |
| 256 | 2116us | Conversion Ready |
| 512 | 4156us |
| 1024 | 8244us |

For which options best fit your use case and how to use them, view the datasheet for the INA226 and determine what fits your application.  This example uses 4 samples averaged and a conversion time of 2116us for both shunt and bus voltage sampling.

## References

An example of this device used in an arduino based circuit with a rather comprehensive (however not in c#) covering of the datasheet and use case by Giovanni Carrera [The ArduINA226 power monitor ](https://ardupiclab.blogspot.com/2020/03/the-arduina226-power-monitor.html)
