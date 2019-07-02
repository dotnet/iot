# MPU9250 - 3 axis Gyroscope, 3 axis Accelerometer, 3 axis  Magnetometer and Temperature

MPU9250 is a 3 axis Gyroscope, 3 axis Accelerometer, 3 axis  Magnetometer and Temperature sensor that can be accessed either thru I2C or SPI. This implementation is only for I2C. The sensor can be found in various implementation like [Grove](http://wiki.seeedstudio.com/Grove-IMU_9DOF_v2.0/) or [Sparkfun](https://www.sparkfun.com/products/13762). 

The Magnetometer used is an [AK8963](../Ak8963/README.md). It is managed thru the main MPU9250 and setup as a slave I2C. All operations go thru the MPU9250.

## Uage

You can find an example in the [sample](./samples/Mpu9250.sample.cs) directory. Usage is straightforward including the possibility to have a calibration for all sub sensors.

```csharp
var mpui2CConnectionSettingmpus = new I2cConnectionSettings(1, Mpu9250.DefaultI2cAddress);
Mpu9250 mpu9250 = new Mpu9250(I2cDevice.Create(mpui2CConnectionSettingmpus));
Console.WriteLine($"Check version: {mpu9250.CheckVersion()}");
var gyro = mpu9250.Gyroscope;
Console.WriteLine($"Gyro X = {gyro.X, 15}");
Console.WriteLine($"Gyro Y = {gyro.Y, 15}");
Console.WriteLine($"Gyro Z = {gyro.Z, 15}");
var acc = mpu9250.Accelerometer;
Console.WriteLine($"Acc X = {acc.X, 15}");
Console.WriteLine($"Acc Y = {acc.Y, 15}");
Console.WriteLine($"Acc Z = {acc.Z, 15}");
Console.WriteLine($"Temp = {mpu9250.Temperature.Celsius.ToString("0.00")} °C");
var magne = mpu9250.Magnetometer;
Console.WriteLine($"Mag X = {magne.X, 15}");
Console.WriteLine($"Mag Y = {magne.Y, 15}");
Console.WriteLine($"Mag Z = {magne.Z, 15}");
```

## Self-test

A self-test is available for the gyroscope and the accelerometer.

```csharp
var resSelfTest = mpu9250.RunGyroscopeAccelerometerSelfTest();
Console.WriteLine($"Self test:");
Console.WriteLine($"Gyro X = {resSelfTest.Item1.X} vs >0.005");
Console.WriteLine($"Gyro Y = {resSelfTest.Item1.Y} vs >0.005");
Console.WriteLine($"Gyro Z = {resSelfTest.Item1.Z} vs >0.005");
Console.WriteLine($"Acc X = {resSelfTest.Item2.X} vs >0.005 & <0.015");
Console.WriteLine($"Acc Y = {resSelfTest.Item2.Y} vs >0.005 & <0.015");
Console.WriteLine($"Acc Z = {resSelfTest.Item2.Z} vs >0.005 & <0.015");
```

The returned data are the raw data and allows you to estimate the quality of the test. The first item of the tuple is the gyroscope and the second one the accelerometer.

No self-test is available for the magnetometer.

## Calibration and bias

You can calibrate the Gyroscope and the Accelerometer at the same time. This action is as well correcting the registers directly in the MPU9250 chip. Those data are lost in case of power stop but stays in case of soft reset.

```csharp
Console.WriteLine("Running Gyroscope and Accelerometer calibration");
mpu9250.CalibrateGyroscopeAccelerometer();
Console.WriteLine("Calibration results:");
Console.WriteLine($"Gyro X bias = {mpu9250.GyroscopeBias.X}");
Console.WriteLine($"Gyro Y bias = {mpu9250.GyroscopeBias.Y}");
Console.WriteLine($"Gyro Z bias = {mpu9250.GyroscopeBias.Z}");
Console.WriteLine($"Acc X bias = {mpu9250.AccelerometerBias.X}");
Console.WriteLine($"Acc Y bias = {mpu9250.AccelerometerBias.Y}");
Console.WriteLine($"Acc Z bias = {mpu9250.AccelerometerBias.Z}");
```

Calibration is as well available for the magnetometer (the AK8963). For this sensor, no correction is done.

```csharp
Console.WriteLine("Magnetometer calibration is taking couple of seconds, please be patient!");
var mag = mpu9250.CalibrateMagnetometer();
Console.WriteLine($"Bias:");
Console.WriteLine($"Mag X = {mpu9250.MagnometerBias.X}");
Console.WriteLine($"Mag Y = {mpu9250.MagnometerBias.Y}");
Console.WriteLine($"Mag Z = {mpu9250.MagnometerBias.Z}");
```

## Units

Al axis are oriented this way:
            +Z   +Y
          \  |  /
           \ | /
            \|/
            /|\
           / | \
          /  |  \
                 +X

### Gyroscope

The unit used for the gyroscope are degree per seconds.

### Accelerometer

The unit used for the accelerometer is G.

### Magnetometer

The unit used for the magnetometer is µTesla.

### Temperature

The Temperature is a normalized Units.Temperature which can provide Celsius, Kelvin or Fahrenheit degrees.

## Measurement modes

The MPU9250 offers a large variety of measurement modes. They can be changed and adjusted thru the properties like:

* ```MagnetometerMeasurementMode``` to adjust the type of measurement for the magnetometer
* ```MagnetometerOutputBitMode``` to select between 14 and 16 bits precision of the magnetometer
* ```AccelerometerRange``` to adjust the range of the accelerometer between 2, 4, 8 or 16 G
* ```AccelerometerBandwidth``` to adjust the frequency of measurement from 5 Hz to 1130 Hz
* ```GyroscopeRange``` to adjust the range of the gyroscope from 250, 500, 1000 and 2000 degrees per second
* ```GyroscopeBandwidth``` to adjust the frequency of measurement from 5 Hz to 8800 Hz
* ```SampleRateDivider``` allows you to reduce the number of samples for the gyroscope and the accelerometer. This feature is only available for some of the bandwidth modes.
* ```DisableModes``` allows you to disable any of the gyroscope and accelerometer axis

### Wake on motion

A unique ```SetWakeOnMotion``` mode is available. It puts the MPU9250 in a low consumption, low measurement rate mode and trigger an interruption on the INT pin. 

```csharp
mpu9250.SetWakeOnMotion(300, AccelerometerLowPowerFrequency.Frequency0Dot24Hz);
// You'll need to attach the INT pin to a GPIO and read the level. Once going up, you have 
// some data and the sensor is awake
// In order to simulate this without a GPIO pin, you will see that the refresh rate is very low
// Setup here at 0.24Hz which means, about every 4 seconds

while (!Console.KeyAvailable)
{
    Console.CursorTop = 0;
    var acc = mpu9250.Accelerometer;
    Console.WriteLine($"Acc X = {acc.X, 15}");
    Console.WriteLine($"Acc Y = {acc.Y, 15}");
    Console.WriteLine($"Acc Z = {acc.Z, 15}");
    Thread.Sleep(100);
}
```

### FIFO mode

The Fifo mode allows you to get the data by batch. You can select the mode thru ```FifoModes```, then read the ```FifoCount``` property. You can then read the data thru ```ReadFifo``` Make sure you'll size the ```Span<byte>``` with ```FifoCount``` length.

Data are in the order of the Register from 0x3B to 0x60 so you'll get your data in this order:
* ACCEL_XOUT_H and ACCEL_XOUT_L
* ACCEL_YOUT_H and ACCEL_YOUT_L 
* ACCEL_ZOUT_H and ACCEL_ZOUT_L
* TEMP_OUT_H and TEMP_OUT_L
* GYRO_XOUT_H and GYRO_XOUT_L
* GYRO_YOUT_H and GYRO_YOUT_L
* GYRO_ZOUT_H and GYRO_ZOUT_L
* EXT_SENS_DATA_00 to EXT_SENS_DATA_24

It is then up to you to transform them into the correct data. You can multiply your raw data by ```AccelerometerConversion``` and ```GyroscopeConvertion``` to convert them properly. 

### I2C Slave primitives

2 primitive functions allow to read and write any register in any of the slave devices. 

* ```I2cWrite(I2cChannel i2cChannel, byte address, byte register, byte data)```
    * i2cChannel: The slave channel to attached to the I2C device
    * address: The I2C address of the slave I2C element
    * register: The register to write to the slave I2C element
    * data: The byte data to write to the slave I2C element
* ```I2cRead(I2cChannel i2cChannel, byte address, byte register, Span<byte> readBytes)```
    * i2cChannel: The slave channel to attached to the I2C device
    * address: The I2C address of the slave I2C element
    * register: The register to write to the slave I2C element
    * readBytes: The read data

## Circuit

The following fritzing diagram illustrates one way to wire up the MPU9250 with a Raspberry Pi using I2C.

![Raspberry Pi Breadboard diagram](./samples/Mpu9250_bb.png)

## Reference

* Registers: http://www.invensense.com/wp-content/uploads/2017/11/RM-MPU-9250A-00-v1.6.pdf
* Product specifications: http://www.invensense.com/wp-content/uploads/2015/02/PS-MPU-9250A-01-v1.1.pdf

