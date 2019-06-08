using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;

namespace Iot.Device.Samples
{
    /// <summary>
    /// Sample program for reading <see cref="Bme680"/> sensor data on a Raspberry Pi.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Hello BME680!");

            // The I2C bus ID on the Raspberry Pi 3.
            const int busId = 1;

            var i2cConnectionSettings = new I2cConnectionSettings(busId, Bme680.DefaultI2cAddress);
            var unixI2cDevice = new UnixI2cDevice(i2cConnectionSettings);

            using (var bme680 = new Bme680(unixI2cDevice))
            {
                bme680.SetHumiditySampling(Sampling.UltraLowPower);
                bme680.SetTemperatureSampling(Sampling.LowPower);
                bme680.SetPressureSampling(Sampling.UltraHighResolution);

                while (true)
                {
                    // Once a reading has been taken, the sensor goes back to sleep mode.
                    if (bme680.ReadPowerMode() == Bme680PowerMode.Sleep)
                    {
                        // This instructs the sensor to take a measurement.
                        bme680.SetPowerMode(Bme680PowerMode.Forced);
                    }

                    // This prevent us from reading old data from the sensor.
                    if (bme680.HasNewData)
                    {
                        var temperature = Math.Round(bme680.Temperature.Celsius, 2).ToString("N2");
                        var pressure = Math.Round(bme680.Pressure / 100, 2).ToString("N2");
                        var humidity = Math.Round(bme680.Humidity, 2).ToString("N2");

                        Console.WriteLine($"{temperature} °c | {pressure} hPa | {humidity} %rH");

                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
