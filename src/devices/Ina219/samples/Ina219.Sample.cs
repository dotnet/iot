// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using Iot.Device;

namespace Iot.Device.Adc.Samples
{
    /// <summary>
    /// Sample program for Ina219
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            const byte Adafruit_Ina219_I2cAddress = 0x40;
            const byte Adafruit_Ina219_I2cBus = 0x1;

            // create an Ina219 device on I2c bus 1 addressing channel 64
            using (Ina219 device = new Ina219(new I2cConnectionSettings(Adafruit_Ina219_I2cBus, Adafruit_Ina219_I2cAddress)))
            {
                // reset the device 
                device.Reset();

                // set the calibration to have a +/- 100mA range
                device.SetCalibration(Ina219.PgaSensitivity.PlusOrMinus40mv, 0.1F);
                while (true)
                {
                    // write out the current values from the Ina219 device.
                    System.Console.WriteLine($"Bus Voltage {device.GetBusVoltage()}V Shunt Voltage {device.GetShuntVoltage() * 1000}mV Current {device.GetCurrent() * 1000}mA Power {device.GetPower() * 1000}mW");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
