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

* Fritzing Diagram
* SMBUS Alert Response (feature has not been implemented)
* Alert pin related telemetry for the three different alert flags still needs to be tested with hardware.
* Input validation on current and power properties to prevent out of range exceptions

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

The INA226 allows for specifying the number of samples to average as well as the conversion time for both shunt voltage measurement and bus voltage measurements.

### Defaults allow some functionality (Initialize the object)

The Ina226 has default values for each of these with the exception of the calibration value used to scale the shunt and bus voltage readings into the current and power registers. Therefore if you only intend to use the shunt and bus voltage registers, then you only need to create the Ina226 object with the correct i2c settings to make use of the device.

### Foundation of accuracy (Properties)

To get the accuracy from the Ina226 that we expect, as well as full functionality, we cannot run it at the default settings. Lets go through each property and choose the option that best fits this example.

#### Samples to Average

The Ina226 allows for 8 different choices on the number of samples to average prior to writing the values of the different registers.  Using the follow code line, you can set the sample averaging quantity to the configuartion register, just replace the '*' with the value you want to set.

```csharp

ina226.SamplesAveraged = Ina226SamplesAveraged.Quantity_*;

```

| Samples To Average |
| :--- |
| 1 |
| 4 |
| 16 |
| 64 |
| 128 |
| 256 |
| 512 |
| 1024 |

#### Shunt and Bus Voltage sample conversion time

When configuring the Ina226 for accuracy you are going to want to give it as much time as your application allows.  In this sample we are choosing a middle ground option leaning towards slower for more accuracy (less noise) of 2116 microseconds. Replace the '*'s with the number value of microseconds in the table below into each of the properties you want to set.  You can set each of these to different values allowing for higher frequency monitoring of a shunt in a system with a fairly stable bus voltage for example.

```csharp

ina226.ShuntConvTime = Ina226ShuntConvTime.Time***us;

```

```csharp

ina226.BusConvTime = Ina226BusVoltageConvTime.Time***us;

```

| Sample Conversion Time |
| :--- |
| 140 |
| 204 |
| 332 |
| 588 |
| 1100 |
| 2116 |
| 4156 |
| 8244 |

#### Setting calibration

To set the calibration we only need to know the resistance of the shunt in ohms, and the maximum current at 81.92mv of voltage difference.  You can technically calculate one of these from the other, however one of these values is stored on the ina226 and the other in the software and is meant to allow for you to have software correction to the calibration if needed. If doing software correction, do so by changing the maximum current expected, not the resistance of the shunt. The values below are for a 100amp 75mv shunt.

```csharp

ina226.SetCalibration(0.00075, 109.226);

```

### Alert Pin Configuartion Properties (can be ignored if not useing the alert pin)

#### Alert Pin Operation Modes

The Alert Pin operation mode can be configured using the following command.  The alert pin can be used in a number of ways of which a few I can document here, for the further complex modes the datasheet would be the better resource for consideration. Replacing the '*'s with the mode you wish to use.

Be aware, only one AlertMode can be set at a time.  While in theory you could enable each modes bit, only the highest significant bit of the different bit modes, would be used for the alert trigger mode.  The way the AlertMode property works, you cannot set two different modes at once, choosing a different mode simply changes the enabled mode bit.

```csharp

ina226.AlertMode = Ina226AlertMode.****;

```

| Alert Pin Operating Modes | Explanation of function |
| :--- | :--- |
| ShuntOverVoltage | When the value of trigger limit register is LOWER than the shunt voltage register the trigger pin will be pulled low when polarity is set to default |
| ShuntUnderVoltage | When the value of trigger limit register is HIGHER than the shunt voltage register the trigger pin will be pulled low when polarity is set to default |
| BusOverVoltage | When the value of the trigger limit register is LOWER than the bus voltage register the trigger pin will be pulled low when polarity is set to default |
| BusUnderVoltage | When the value of the trigger limit register is HIGHER than the bus voltage register the trigger pin will be pulled low when polarity is set to default |
| PowerOverLimit | When the value of the trigger limit register is LOWER than the power register the trigger pin will be pulled low when polarity is set to default |
| ConversionReady | When the conversion ready bit is enabled, the trigger pin will be pulled low each time a conversion is finished. (this will also trigger the conversion ready flag bit.) |

#### Alert Trigger Limit

The Alert limit trigger register contains the value which triggers the alert pin depending on how you have configured the alert pin operation mode.  When configuring this for bus voltage the following line of code will write a bus voltage limit of 14.25 volts.

```csharp

ina226.AlertLimitVoltage = UnitsNet.ElectricPotential.FromVolts(14.25);

```

When using the shunt specific operating mode, this changes a little.  We are storing the value in the same register but adjusting the value to match the scale of the shunt voltage.

```csharp

ina226.AlertLimitCurrent = UnitsNet.ElectricCurrent.FromAmperes(5);

```

When using the power over limit operating mode, this changes a little as well, in this case to match the power register scaling. Here we will set the maximum for 2500 Watts.

```csharp

ina226.AlertLimitPower = UnitsNet.Power.FromWatts(2500);

```

#### Alert Polarity

The alert polarity can either be set to false or true which correspond to normal and inverted accordingly.  Normal operation being active-low open collector, and inverted which is active-high open collector. In the following line we set the polarity to inverted.

```csharp

ina226.AlertPolarity = true;

```

#### Alert Latching

The alert latching feature allows for the alert to be held until the alert register has been read, however we are going to leave it the default (false) which leaves the latch transparent to the circuit. Meaning the alert pin is reset once the alert is cleared instead of held for the register to be read. This line of code is needed if you want the latch enabled, just leave out for default.

```csharp

ina226.AlertLatchEnable = true;

```

### Telemetry

| Function | Description |
| :--- | :--- |
| ReadShuntVoltage() | Returns an ElectricalPotential reading from the Shunt Voltage register |
| ReadBusVoltage() | Returns an ElectricalPotential reading from the Bus Voltage register |
| ReadCurrent() | Returns an ElectricCurrent reading from the Current register |
| ReadPower() | Returns a Power object holding the value of the Power register |
| AlertFunctionFlag() | Returns a boolean of the Alert function flag status |
| ConversionReadyFlag() | Returns a boolean of the Conversion Ready flag status |
| MathOverflowFlag() | Returns a boolean of the MathOverflow flag status |

For which options best fit your use case and how to use them, view the datasheet for the INA226 and determine what fits your application.  The sample project uses 4 samples averaged and a conversion time of 2116us for both shunt and bus voltage sampling.

## References

An example of this device used in an arduino based circuit with a rather comprehensive (however not in c#) covering of the datasheet and use case by Giovanni Carrera: The [ArduINA226](https://ardupiclab.blogspot.com/2020/03/the-arduina226-power-monitor.html) power monitor 
