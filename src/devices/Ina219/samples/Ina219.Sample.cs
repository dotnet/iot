// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using Iot.Device;

namespace Iot.Device.Adc.Samples
{
    /// <summary>
    /// Sample program for INA219
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            const byte Adafruit_Ina219_I2cAddress = 0x40;
            const byte Adafruit_Ina219_I2cBus = 0x1;

            // create an INA219 device on I2C bus 1 addressing channel 64
            using (Ina219 device = new Ina219(new I2cConnectionSettings(Adafruit_Ina219_I2cBus, Adafruit_Ina219_I2cAddress)))
            {
                // reset the device 
                device.Reset();

                // set up the bus and shunt voltage ranges and the calibration. Other values left at default.
                device.BusVoltageRange = Ina219BusVoltageRange.Range16v;
                device.PgaSensitivity = Ina219PgaSensitivity.PlusOrMinus40mv;
                device.SetCalibration(33574, (float)12.2e-6);

                while (true)
                {
                    // write out the current values from the INA219 device.
                    System.Console.WriteLine($"Bus Voltage {device.ReadBusVoltage()}V Shunt Voltage {device.ReadShuntVoltage() * 1000}mV Current {device.ReadCurrent() * 1000}mA Power {device.ReadPower() * 1000}mW");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
