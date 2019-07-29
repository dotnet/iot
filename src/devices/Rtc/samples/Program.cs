using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Rtc;

namespace RealtimeClock.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // This project contains DS1307, DS3231, PCF8563

            I2cConnectionSettings settings = new I2cConnectionSettings(1, Ds3231.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Ds3231 rtc = new Ds3231(device))
            {
                // set time
                rtc.DateTime = DateTime.Now;

                // loop
                while (true)
                {
                    // read time
                    DateTime dt = rtc.DateTime;

                    Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
                    Console.WriteLine($"Temperature: {rtc.Temperature.Celsius} ℃");
                    Console.WriteLine();

                    // wait for a second
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
