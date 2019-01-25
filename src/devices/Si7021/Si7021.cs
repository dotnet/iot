// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device
{
    public class Si7021 : IDisposable
    {
        private const byte ReadTemperatureCommand = 0xE3;
        private const byte ReadHumidityCommand = 0xE5;
        private I2cDevice _i2cDevice;

        public Si7021(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public void Dispose()
        {
        }

        public double ReadTemperatureInFahrenheit()
        {
            double tempCelcius = ReadTemperatureInCelcius();
            double tempFahrenheit = (tempCelcius * (9 / 5)) + 32;
            
            return tempFahrenheit;
        }
        
        public double ReadTemperatureInCelcius()
        {
            byte[] buffer = new byte[2];

            // Send temperature command, read back two bytes
            _i2cDevice.WriteByte(ReadTemperatureCommand);
            _i2cDevice.Read(buffer.AsSpan());
            
            // Calculate temperature
            double temp_code = buffer[0] << 8 | buffer[1];
            double tempCelcius = (((175.72 * temp_code) / 65536) - 46.85);
            
            return tempCelcius;
        }
        
        public double ReadHumidity()
        {
            byte[] buffer = new byte[2];
            
            // Send humidity read command, read back two bytes
            _i2cDevice.WriteByte(ReadHumidityCommand);
            _i2cDevice.Read(buffer.AsSpan());
            
            // Calculate humidity
            double rh_code = buffer[0] << 8 | buffer[1];
            double humidity = ((125 * rh_code) / 65536) - 6;
            
            return humidity;
        }
    }
}
