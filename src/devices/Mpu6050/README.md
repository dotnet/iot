# Mpu6050 - accelerometer and gyroscope

The MPU-6050 is a 6-axis motion tracking sensor that combines a 3-axis gyroscope and a 3-axis accelerometer including the following features:

- gyroscope programmable FSR of ±250 dps, ±500 dps, ±1000 dps, and ±2000 dps
- accelerometer with programmable FSR of ±2g, ±4g, ±8g, and ±16g

## Documentation

- [Datasheet](https://invensense.tdk.com/wp-content/uploads/2015/02/MPU-6000-Datasheet1.pdf)

## Usage

Create the `Mpu6050` class and pass the I2C device. By default the I2C address for this sensor is `0x68`.

```csharp
I2cConnectionSettings settings = new(busId: 1, deviceAddress: Mpu6050AccelerometerGyroscope.DefaultI2cAddress);
using (Mpu6050AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
{
    Console.WriteLine($"Internal temperature: {ag.GetInternalTemperature().DegreesCelsius} C");

    while (!Console.KeyAvailable)
    {
        var acc = ag.GetAccelerometer();
        var gyr = ag.GetGyroscope();
        Console.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Console.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
        Thread.Sleep(100);
    }
}
```

### Sample output

```text
Internal temperature: 64.21664626 C
Accelerometer data x:-0.041503906 y:0 z:1.056884765
Gyroscope data x:4.94384765 y:-8.60595703 z:-15.68603515

Accelerometer data x:-0.040771484 y:-0.0051269531 z:1.062988281
Gyroscope data x:4.94384765 y:-7.56835937 z:-15.014648437

Accelerometer data x:-0.046630859 y:-0.0068359375 z:1.055175781
Gyroscope data x:3.60107421 y:-7.62939453 z:-15.1977539

Accelerometer data x:-0.049560546 y:-0 z:1.061279296
Gyroscope data x:4.39453125 y:-7.32421875 z:-14.28222656
```

See [samples](samples) for a complete sample application.

## Sleep mode

The sensor can be put in sleep mode by calling the `Sleep` function. After that the `WakeUp` function should be called.

## Setting scales

Both for the gyroscope and accelerometer you can set the desired scales using the `AccelerometerScale` and `GyroscopeScale`
properties.

```csharp
// Set the scale of the accelerometer to 2G
ag.AccelerometerScale = AccelerometerScale.Scale2G;
```

## Setting enabled axes

By default all gyroscope and accelerometer axes are enabled. If desired you can specify explicitly which axes should be enabled.
In that case, all other axes will be disabled. When an axis is disabled, data can still be read for that axis but the values will not
change anymore.

```csharp
// Enable only the X axis of the gyroscope and accelerometer
ag.EnabledAxes = EnabledAxis.GyroscopeX | EnabledAxis.AccelerometerX;
```

## Features not implemented

The following features are enabled in the MPU6050 but not yet implemented in this driver:

- registers `0x0d` to `0x0F`: self test
- register `0x1a`: external Frame Synchronization
- register `0x36`: fsync interrupt status
- register `0x37`: INT/DRDY pin config
- register `0x68`: signal path reset
