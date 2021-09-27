# Mpu6886 - accelerometer and gyroscope

The MPU-6886 is a 6-axis motion tracking sensor that combines a 3-axis gyroscope and a 3-axis accelerometer including the following features:

- gyroscope programmable FSR of ±250 dps, ±500 dps, ±1000 dps, and ±2000 dps
- accelerometer with programmable FSR of ±2g, ±4g, ±8g, and ±16g

## Documentation

- [Datasheet](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/MPU-6886-000193%2Bv1.1_GHIC_en.pdf), register descriptions start at page 32.

## Usage

Create the `Mpu6886` class and pass the I2C device. By default the I2C address for this sensor is `0x68`.

```csharp
I2cConnectionSettings settings = new(busId: 1, deviceAddress: Mpu6886AccelerometerGyroscope.DefaultI2cAddress);
using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
{
    Console.WriteLine("Start calibration ...");
    var offset = ag.Calibrate(1000);
    Console.WriteLine($"Calibration done, calculated offsets {offset.X} {offset.Y} {offset.Y}");

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
Start calibration ...
Calibration done, calculated offsets 49.189 -86.21099999 -86.21099999
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

## Calibration

The gyroscope can be calibrated using the `Calibrate` function. With the `iterations` parameter
you can specify how many values should be read from the sensor to calculate an average (1000 iterations seems
to be a good number). During the calibration you want to keep the sensor still and in a fixed position (e.g. lying
flat on a table). The `Calibrate` function will write the values to the corresponding compensation registers
of the sensor, values are corrected when retrieved automatically.

The return value of the `Calibrate` method gives a vector containing the 3 (for the X,Y and Z axes) calculated
compensation values.

```csharp
var offset = ag.Calibrate(1000);
Debug.WriteLine($"Calibration done, calculated offsets {offset.X} {offset.Y} {offset.Y}");
```

It is possible to write directly to the compensation registers using the `SetGyroscopeOffset` function. You might
want to do this when you create your own custom calibration method, or you retrieve existing calibration data from
a persisted data store or memory location.

⚠ **When the devices reboots, the offset registers are cleared.** So if you don't persist the calibration data yourself
you will have to run the calibration method every time the device boots.

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

The following features are enabled in the MPU6886 but not yet implemented in this driver:

- registers `0x05` to `0x0b`: low noise to low power offset
- registers `0x0d` to `0x0F`: self test
- register `0x1a`: FIFO mode, DLPF (digital low pass filter)
- register `0x1d` bit 0 to 3: DLPF
- register `0x1e` bit 4 to 6: averaging filter for low power mode
- registers `0x20` to `0x22`: threshold for wake-on motion
- register `0x36`: fsync interrupt status
- register `0x37`: INT/DRDY pin config
- register `0x39` and `0x3a`: FIFO watermark interrupt
- register `0x60` and `0x61`: FIFO watermark interrupt
- register `0x68`: signal path reset
- register `0x69`: acc. intelligence control
