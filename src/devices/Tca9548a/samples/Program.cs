// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Tca9548a;
using Iot.Device.Bno055;
using Iot.Device.Bmp180;
using UnitsNet;
using Iot.Device.Common;

namespace Tca9548a.Sample
{
    /// <summary>
    /// Sample Test Class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program Entry Point
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("Hello TCA9548A!");
            I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Tca9548A.DefaultI2cAddress));
            Tca9548A tca9548a = new Tca9548A(i2cDevice);

            // Get all connected I2C interfaces
            var connectedDevices = tca9548a.ScanAllChannelsForDeviceAddress();

            foreach (var channelDevices in connectedDevices)
            {
                // Important to switch channel before connecting to that sensor
                tca9548a.SelectChannel(channelDevices.Key);

                if (tca9548a.TryGetSelectedChannel(out Channels selectedChannel))
                {
                    Console.WriteLine($"Selected Channel on MUX: {selectedChannel}");
                }

                foreach (var device in channelDevices.Value)
                {
                    if (device == Bno055Sensor.DefaultI2cAddress || device == Bno055Sensor.SecondI2cAddress)
                    {
                        I2cDevice bnoI2cDevice = I2cDevice.Create(new I2cConnectionSettings(i2cDevice.ConnectionSettings.BusId, device));
                        Bno055Sensor bno055Sensor = new Bno055Sensor(bnoI2cDevice);
                        Console.WriteLine($"Id: {bno055Sensor.Info.ChipId}, AccId: {bno055Sensor.Info.AcceleratorId}, GyroId: {bno055Sensor.Info.GyroscopeId}, MagId: {bno055Sensor.Info.MagnetometerId}");
                        Console.WriteLine($"Firmware version: {bno055Sensor.Info.FirmwareVersion}, Bootloader: {bno055Sensor.Info.BootloaderVersion}");
                        Console.WriteLine($"Temperature source: {bno055Sensor.TemperatureSource}, Operation mode: {bno055Sensor.OperationMode}, Units: {bno055Sensor.Units}");
                        Console.WriteLine($"Powermode: {bno055Sensor.PowerMode}");
                        var calibrationStatus = bno055Sensor.GetCalibrationStatus();
                        Console.WriteLine($"Calibration Status : {calibrationStatus}");
                        var magneto = bno055Sensor.Magnetometer;
                        Console.WriteLine($"Magnetometer X: {magneto.X} Y: {magneto.Y} Z: {magneto.Z}");
                        var gyro = bno055Sensor.Gyroscope;
                        Console.WriteLine($"Gyroscope X: {gyro.X} Y: {gyro.Y} Z: {gyro.Z}");
                        var accele = bno055Sensor.Accelerometer;
                        Console.WriteLine($"Acceleration X: {accele.X} Y: {accele.Y} Z: {accele.Z}");
                        var orien = bno055Sensor.Orientation;
                        Console.WriteLine($"Orientation Heading: {orien.X} Roll: {orien.Y} Pitch: {orien.Z}");
                        var line = bno055Sensor.LinearAcceleration;
                        Console.WriteLine($"Linear acceleration X: {line.X} Y: {line.Y} Z: {line.Z}");
                        var gravity = bno055Sensor.Gravity;
                        Console.WriteLine($"Gravity X: {gravity.X} Y: {gravity.Y} Z: {gravity.Z}");
                        var qua = bno055Sensor.Quaternion;
                        Console.WriteLine($"Quaternion X: {qua.X} Y: {qua.Y} Z: {qua.Z} W: {qua.W}");
                        var temp = bno055Sensor.Temperature.DegreesCelsius;
                        Console.WriteLine($"Temperature: {temp} °C");
                    }
                    else if (device == Bmp180.DefaultI2cAddress)
                    {
                        I2cConnectionSettings i2cSettings = new I2cConnectionSettings(i2cDevice.ConnectionSettings.BusId, Bmp180.DefaultI2cAddress);
                        using I2cDevice bmpDevice = I2cDevice.Create(i2cSettings);
                        using Bmp180 i2cBmp280 = new(bmpDevice);
                        // set samplings
                        i2cBmp280.SetSampling(Sampling.Standard);
                        // read values
                        Temperature tempValue = i2cBmp280.ReadTemperature();
                        Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
                        Pressure preValue = i2cBmp280.ReadPressure();
                        Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

                        // Note that if you already have the pressure value and the temperature, you could also calculate altitude by
                        // calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
                        Length altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);

                        Console.WriteLine($"Altitude: {altValue:0.##}m");
                        Thread.Sleep(1000);

                        // set higher sampling
                        i2cBmp280.SetSampling(Sampling.UltraLowPower);

                        // read values
                        tempValue = i2cBmp280.ReadTemperature();
                        Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
                        preValue = i2cBmp280.ReadPressure();
                        Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");

                        // Note that if you already have the pressure value and the temperature, you could also calculate altitude by
                        // calling WeatherHelper.CalculateAltitude(preValue, Pressure.MeanSeaLevel, tempValue) which would be more performant.
                        altValue = i2cBmp280.ReadAltitude(WeatherHelper.MeanSeaLevel);
                        Console.WriteLine($"Altitude: {altValue:0.##}m");
                    }

                    Thread.Sleep(1000);
                }

            }
        }
    }
}
