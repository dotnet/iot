# Ina226 Sample Project

## Summary

This is a sample project which uses the same example code in the binding readme. This readme will be a further explanation of each of the properties, telemetry, and commands in this binding.

## Explanation of Ina226 Setup

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
