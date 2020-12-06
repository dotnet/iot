# TSL256x - Illuminance sensor

TSL2560 and TSL2561 are illuminance sensor. They are light-to-digital converters that transform light intensity to a digital signal output capable of direct I2C (TSL2561) or
SMBus (TSL2560) interface. Each device combines one broadband photodiode (visible plus infrared) and one infrared-responding photodiode on a single CMOS integrated circuit capable of providing a near-photopic response over an effective 20-bit dynamic range (16-bit resolution).

Two integrating ADCs convert the photodiode currents to a digital output that represents the irradiance measured on each channel. This digital output can be input to a microprocessor where illuminance (ambient light level) in lux is derived using an empirical formula to approximate the human eye response.

The TSL2560 device permits an SMB-Alert style interrupt, and the TSL2561 device supports a traditional level style interrupt that remains asserted until the firmware
clears it.

## Basic usage

TSL2560 and TSL2561 are designed to have an integration time and a gain for measuring the 2 ADC. The defaults are 402 milliseconds and a normal gain of x1.

The basic usage is the following:

```csharp
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Tsl256x.DefaultI2cAddress));
Tsl256x tsl256X = new(i2cDevice, PackageType.Other);
tsl256X.IntegrationTime = IntegrationTime.Integration402Milliseconds;
tsl256X.Gain = Gain.Normal;
var lux = tsl256X.MeasureAndGetIlluminance();
Console.WriteLine($"Illuminance is {lux.Lux} Lux");
```

**notes**:

- Be aware, there are 2 types of packaging the CS and the Others T, FN and CL. Refer to the documentation to understand which package you hav on the board. This is an argument when creating the sensor as the calculation for the illuminance is different.
- There are 3 different possible I2C addresses for this device depending on how the address pin is setup:
  - DefaultI2cAddress = 0x29: When address pin is to the ground
  - SecondI2cAddress = 0x39: When address pin is floating
  - ThirdI2cAddress = 0x49: When address pin is to VDD

## Check the version

You can determine if you have a TSL2560 or TSL2561 version:

```csharp
var ver = tsl256X.Version;
string msg = ver.Major == 1 ? $"This is a TSL2561, revision {ver.Minor}" : $"This is a TSL2560, revision {ver.Minor}";
Console.WriteLine(msg);
```

## Using interruptions

You can set interruptions, you have different possible ones you can set thru the `InterruptLevel`:

- OutputDisabled: will disable the interrupt
- LevelInterrupt: will use the permanent interrupt you'll setup, see right after
- SmbAlertCompliant: will send an alert on the SMB bus (version TSL2560 only) on address 0b0000_1100
- TestMode: will generate an interruption for test purpose

The `InterruptPersistence` will allow you to select a specific timing for the interrupt:

- EveryAdc: each time you have a measurement
- AnyValueOutsideThreshold: any time a value will be outside of the setup threshold
- OutOfRangeXXIntegrationTimePeriods: where XX is from 2 to 15, will interrupt only if the value remains outside more than XX times

The following example shows how to setup a basic interruption using test mode:

```csharp
Console.WriteLine("Set interruption to test. Read the interrupt pin");
GpioController controller = new();
controller.OpenPin(PinInterrupt, PinMode.Input);
tsl256X.InterruptControl = InterruptControl.TestMode;
tsl256X.Power = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

tsl256X.Power = false;
Console.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.GetRawChannels(out ushort ch0, out ushort ch1);
```

You can setup interruptions on thresholds as well. You first need to setup a threshold and then the type of interruptions.

```csharp
// Adjust those values with a previous measurement to understand the conditions, find a level where then you can
// hide the sensor with your arm and make it going under the minimum level or vice versa with a lamp
tsl256X.SetThreshold(0x0000, 0x00FF);
tsl256X.InterruptPersistence = InterruptPersistence.OutOfRange06IntegrationTimePeriods;
tsl256X.InterruptControl = InterruptControl.LevelInterrupt;
tsl256X.Power = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

Console.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.Power = false;
tsl256X.GetRawChannels(out ch0, out ch1);
Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
```

## Manual integration

You can set a manual integration as well. Be aware that you won't be able to easily calculate an illuminance equivalent. You need to use the manual integration functions offered:

```csharp
Console.WriteLine("This will use a manual integration for 2 seconds");
tsl256X.StartManualIntegration();
Thread.Sleep(2000);
tsl256X.StopManualIntegration();
tsl256X.GetRawChannels(out ch0, out ch1);
Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
```

## References

- Documentation: https://cdn-shop.adafruit.com/datasheets/TSL2561.pdf