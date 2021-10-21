# AXP192 - Enhanced single Cell Li-Battery and Power System Management IC

## Documentation

- Product documentation can be found [here](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/AXP192_datasheet_en.pdf).
- Registers can be found [here](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/AXP192_datasheet_cn.pdf) (Chineese only, note: bing translator make miracle).
- This sensor is used in the [M5Stick](https://docs.m5stack.com/en/core/m5stickc). Initialization code for this device can be found [here](https://github.com/m5stack/M5StickC/blob/master/src/AXP192.cpp).

## Usage

```csharp
I2cDevice i2cAxp192 = I2cDevice.Create(new I2cConnectionSettings(1, Axp192.I2cDefaultAddress));
Axp192 power = new Axp192(i2cAxp192);
```

> **Important**: make sure you read th documentation of your battery and setup the proper charging values, stop current. Overcharging your battery may damage it.

### Using the button

One button is available and can be setup to track short and long press:

```csharp
// This part of the code will handle the button behavior
power.EnableButtonPressed(ButtonPressed.LongPressed | ButtonPressed.ShortPressed);
power.SetButtonBehavior(LongPressTiming.S2, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms32, ShutdownTiming.S10);
```

The status is kept in the registers up to the next status read. You can then have both a short and a long press, you can get the status like this:

```csharp
var status = power.GetButtonStatus();
if ((status & ButtonPressed.ShortPressed) == ButtonPressed.ShortPressed)
{
    Console.WriteLine("Short press");
}
else if ((status & ButtonPressed.LongPressed) == ButtonPressed.LongPressed)
{
    Console.WriteLine("Long press");
}
```

### Battery status

You can get various elements regarding the battery status:

```csharp
Console.WriteLine($"Battery:");
Console.WriteLine($"  Charge curr  : {power.GetBatteryChargeCurrent().Milliamperes} mA");
Console.WriteLine($"  Status       : {power.GetBatteryChargingStatus()}");
Console.WriteLine($"  Dicharge curr: {power.GetBatteryDischargeCurrent().Milliamperes} mA");
Console.WriteLine($"  Inst Power   : {power.GetBatteryInstantaneousPower().Milliwatts} mW");
Console.WriteLine($"  Voltage      : {power.GetBatteryVoltage().Volts} V");
Console.WriteLine($"  Is battery   : {power.IsBatteryConnected()} ");
```

### Advanced features

The AXP192 can charge the battery, get and set charging current, cut off voltage, has protection for temperature. Most feature can be access or setup. You can check out the [sample](./samples) to get more details on how to set those advance features.

> Note: this binding uses UnitsNet for the units like Voltage, Amperes.

Here is an example reading the current, voltage:

```csharp
Console.WriteLine($"Temperature : {power.GetInternalTemperature().DegreesCelsius} °C");
Console.WriteLine($"Input:");
// Note: the current and voltage will show 0 when plugged into USB.
// To see something else than 0 you have to unplug the charging USB.
Console.WriteLine($"  Current   : {power.GetInputCurrent().Milliamperes} mA");
Console.WriteLine($"  Voltage   : {power.GetInputVoltage().Volts} V");
Console.WriteLine($"  Status    : {power.GetInputPowerStatus()}");
Console.WriteLine($"  USB volt  : {power.GetUsbVoltageInput().Volts} V");
Console.WriteLine($"  USB Curr  : {power.GetUsbCurrentInput().Milliamperes} mA");
```

### Coulomb counter

The AXP192 has a Coulomb counter where the value range is in milli Amperes per hour. You first have to enable the Counter and then you can read the value. It is recommended to let some time between the moment you enable and read the data. Features to reset, stop the count are available as well

```csharp
power.EnableCoulombCounter();
// Do something here
// You can then read periodically the Coulomb counter:
Console.WriteLine($"Coulomb: {power.GetCoulomb()} mA/h");
```
