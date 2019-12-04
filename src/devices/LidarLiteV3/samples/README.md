# LidarLiteV3  LIDAR ToF Sensor by Garmin

# Summary

This device belongs to a class of sensors known as time-of-flight, which measures distances by 
calculating the time delay between signal transmission and reception of the signal as it bounces 
off the subject. Unlike the popular sonar-based HC-SR04, this device uses a low-power laser. The 
advantage is longer distances (up to 40m) but more prone to errors when the subject is reflective. 

# Raspberry PI Wiring

The device communicates over I2C and while there's a PWN mode supported by the device, it is not 
implemented.

I2C Wiring on the Raspberry PI:

| LidarLiteV3 Wire      | Raspberry PI GPIO Pin                   |
|-----------------------|-----------------------------------------|
| 5V (red)              | 5V (pin 2)                              |
| Ground (black)        | Ground (pin 6)                          |
| Power enable (orange) | Optional, an available IO pin (pin 33)  |
| Mode control (yellow) | Not used                                |
| I2C SCL (green)       | SCL (pin 5)                             |
| I2C SDA (blue)        | SDA (pin 3)                             |

> Important! Don't forget to enable I2C via `raspi-config`.

# Simple Example

```csharp
class Program
{
    static void Main(string[] args)
    {
        using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
        {
            // Take 10 measurements, each one second apart.
            for (int i = 0; i < 10; i++) {
                ushort currentDistance = llv3.MeasureDistance();
                Console.WriteLine($"Current Distance: {currentDistance} cm");
                Thread.Sleep(1000);
            }
        }
    }
    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, LidarLiteV3.DefaultI2cAddress);
        return I2cDevice.Create(settings);
    }
}
```

# Advance Usage

## Power Control

Power can be controlled to the device via a GPIO pin. Use the optional constructor parameters to 
specify the GPIO controller and a power enable pin number (numbering scheme depends on GpioController).

```csharp
static void Main(string[] args)
{
    int powerEnablePin = 13;
    using (var llv3 = new LidarLiteV3(CreateI2cDevice(), new GpioController(), powerEnablePin))
    {
        // Power off the device.
        llv3.PowerOff();
        // Device is completely turned off.
        Console.WriteLine("Device is off.");
        Thread.Sleep(5000);
        // Power on the device, device is ready in ~22ms.
        Console.WriteLine("Device is on.");
        llv3.PowerOn();
        // Sleep 50ms.
        Thread.Sleep(50);
        // Get a reading.
        ushort currentDistance = llv3.MeasureDistance();
        Console.WriteLine($"Current Distance: {currentDistance} cm");
    }
}
```

It's also possible to disable the receiver circuit (saving 40 mA) or put the device to sleep 
(saving 20 mA).  However, it's recommended to use the power enable pin instead
since the initialization time is only 2 ms shorter.

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
   llv3.PowerOption = PowerOption.Sleep;
}
```

# Repetition Mode

Instead of getting measurements on-demand, the device can be configure to repeat n number of 
times, or infinitely.

This is configured via `SetMeasurementRepetitionMode` passing in a mode (`Off`, `Repeat`, or 
`RepeatInfinitely`), a loop count (if mode is `Repeat`), and a delay between measurements (default to 
10 hz). A delay of `20` is about `100 hz`.

With a repetition mode set, use `Distance` and `Velocity` to retrieve the current readings in cm.
Negative velocity is toward the device, positive velocity is away the device.  The unit is 
cm and depends on the delay. The default 10 hz is about 0.1 m/s.

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    llv3.SetMeasurementRepetitionMode(MeasurementRepetitionMode.RepeatIndefinitely);

    while(true)
    {
        Thread.Sleep(5);
        ushort currentDistanceInCm = llv3.Distance;
        int currentVelocityInCm = llv3.Velocity;
    }
}
```

# Change the IC2 Address

By default, the device has an address of `0x62`.  It's possible to change this address to 
resolve a conflict with another device or to run multiple devices.

Available addresses are 7-bit values with a 0 in the lsb.

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    // Set device from default `0x62` to `0x68`
   llv3.SetI2CAddress(0x68);
}

// Connect to the device again with the new address.
var settings = new I2cConnectionSettings(1, 0x68);
var i2cDevice = I2cDevice.Create(settings);

using (var llv3 = new LidarLiteV3(i2cDevice))
{
   // ...
}

```

# Optimization

The default settings should work well, but several tweaks can be made to adjust the device.
See the manual for more details.

## Change Acquistion Count

To isolate the signal from the noise, the device performs a series of acquisitions and sums up the 
result until a peak is found.  The number of acquistions can be configured via 
`MaximumAcquisitionCount` (default: 128).

Less acquisitions result in faster measurements, but limits the max range and produces more 
erroneous readings. The number roughly correlates to an acquistion rate of n/count and n^(1/4).  

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    llv3.MaximumAcquisitionCount = 100
}
```

## Quick Termination Mode

Faster acquisition readings, but with slightly more chance of erroneous readings.

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    llv3.AcquistionMode |= AcquistionMode.EnableQuickTermination;
}
```

## Detection Sensitivity

The threshold when a peak is found can be configured via `AlgorithmByPassThreshold`.  By default, 
this is 0 which uses an internal algorithm to determine the threshold.

Recommended non-default values are 32 for higher sensitivity but higher erronenous readings
and 96 for reduced sensitivity with fewer erroneous readings.

```csharp
using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
{
    llv3.AlgorithmBypassThreshold = 32
}
```

# Reference

[Official Manual and Technical Spec](http://static.garmin.com/pumac/LIDAR_Lite_v3_Operation_Manual_and_Technical_Specifications.pdf)