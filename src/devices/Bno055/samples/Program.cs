// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Device.Bno055;

Console.WriteLine("Hello BNO055!");
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bno055Sensor.DefaultI2cAddress));
using Bno055Sensor bno055Sensor = new (i2cDevice);
Console.WriteLine(
    $"Id: {bno055Sensor.Info.ChipId}, AccId: {bno055Sensor.Info.AcceleratorId}, GyroId: {bno055Sensor.Info.GyroscopeId}, MagId: {bno055Sensor.Info.MagnetometerId}");
Console.WriteLine(
    $"Firmware version: {bno055Sensor.Info.FirmwareVersion}, Bootloader: {bno055Sensor.Info.BootloaderVersion}");
Console.WriteLine(
    $"Temperature source: {bno055Sensor.TemperatureSource}, Operation mode: {bno055Sensor.OperationMode}, Units: {bno055Sensor.Units}");
Console.WriteLine($"Powermode: {bno055Sensor.PowerMode}");
Console.WriteLine(
    "Checking the magnetometer calibration, move the sensor up to the calibration will be complete if needed");
while ((bno055Sensor.GetCalibrationStatus() & CalibrationStatus.MagnetometerSuccess) !=
        (CalibrationStatus.MagnetometerSuccess))
{
    Console.Write($".");
    Thread.Sleep(200);
}

Console.WriteLine();
Console.WriteLine("Calibration completed");
while (!Console.KeyAvailable)
{
    Console.Clear();
    Vector3 magneto = bno055Sensor.Magnetometer;
    Console.WriteLine($"Magnetomer X: {magneto.X} Y: {magneto.Y} Z: {magneto.Z}");
    Vector3 gyro = bno055Sensor.Gyroscope;
    Console.WriteLine($"Gyroscope X: {gyro.X} Y: {gyro.Y} Z: {gyro.Z}");
    Vector3 accele = bno055Sensor.Accelerometer;
    Console.WriteLine($"Acceleration X: {accele.X} Y: {accele.Y} Z: {accele.Z}");
    Vector3 orientation = bno055Sensor.Orientation;
    Console.WriteLine($"Orientation Heading: {orientation.X} Roll: {orientation.Y} Pitch: {orientation.Z}");
    Vector3 line = bno055Sensor.LinearAcceleration;
    Console.WriteLine($"Linear acceleration X: {line.X} Y: {line.Y} Z: {line.Z}");
    Vector3 gravity = bno055Sensor.Gravity;
    Console.WriteLine($"Gravity X: {gravity.X} Y: {gravity.Y} Z: {gravity.Z}");
    Vector4 qua = bno055Sensor.Quaternion;
    Console.WriteLine($"Quaternion X: {qua.X} Y: {qua.Y} Z: {qua.Z} W: {qua.W}");
    double temp = bno055Sensor.Temperature.DegreesCelsius;
    Console.WriteLine($"Temperature: {temp} °C");
    Thread.Sleep(100);
}
