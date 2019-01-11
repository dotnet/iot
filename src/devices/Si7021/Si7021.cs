// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device
{
    public class Si7021 : IDisposable
    {
        const byte si7021_cmd_read_temperature = 0xE3;
        private I2cDevice _i2cDevice;

        public Si7021(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public void Dispose()
        {
        }

        public double ReadTempFahrenheit()
        {
            double temp_celcius = ReadTempCelcius();
            double temp_fahrenheit = (ReadTempCelcius() * (9 / 5)) + 32;
            
            return temp_fahrenheit;
        }
        
        public double ReadTempCelcius()
        {
            byte[] buffer = new byte[2];

            // Send temperature command, read back two bytes
            _i2cDevice.WriteByte(si7021_cmd_read_temperature);
            _i2cDevice.Read(buffer.AsSpan());
            
            // Calculate temperature
            double temp_code = buffer[0] << 8 | buffer[1];
            double temp_celcius = (((175.72 * temp_code) / 65536) - 46.85);
            
            return temp_celcius;
        }
    }
}
