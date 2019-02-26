# MPR121

## Summary
The 12-channels I2C proximity capacitive touch sensor controller.

## Device Family

**MPR121**: https://www.sparkfun.com/datasheets/Components/MPR121.pdf


## Binding Notes

The binding provides different options of device configuration. The device can be configured to update the channel statuses periodically. Also it supports custom configuration of controller registers.

#### Default configuration with manually updating of channel statuses

```csharp
var i2cDevice = new UnixI2cDevice(new I2cConnectionSettings(busId: 1, deviceAddress: 0x5A));
var mpr121 = new Mpr121(i2cDevice);

mpr121.RefreshChannelStatuses();
var status = mpr121.ChannelStatuses[Channels.CH_1]
    ? "pressed"
    : "released";

Console.WriteLine($"The 1st channel is {status}");
```

#### Channel statuses auto refresh

```csharp
var i2cDevice = new UnixI2cDevice(new I2cConnectionSettings(busId: 1, deviceAddress: 0x5A));

// Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
var mpr121 = new Mpr121(i2cDevice, 100);

// Subscribe to channel statuses updates.
mpr121.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
    if (e.PropertyName == nameof(Mpr121.ChannelStatuses))
    {
        // do something.
    }
};
```

#### Custom MPR121 registers configuration

```csharp
var i2cDevice = new UnixI2cDevice(new I2cConnectionSettings(busId: 1, deviceAddress: 0x5A));
var config = new Mpr121Configuration
{
    MaxHalfDeltaRising = 0x01,
    NoiseHalfDeltaRising = 0x01,
    NoiseCountLimitRising = 0x00,
    FilterDelayCountLimitRising = 0x00,
    MaxHalfDeltaFalling = 0x01,
    NoiseHalfDeltaFalling = 0x01,
    NoiseCountLimitFalling = 0xFF,
    FilterDelayCountLimitFalling = 0x01,
    ElectrodeTouchThreshold = 0x0F,
    ElectrodeReleaseThreshold = 0x0A,
    ChargeDischargeTimeConfiguration = 0x04,
    ElectrodeConfiguration = 0x0C
};

var mpr121 = new Mpr121(i2cDevice, config);
```