// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-si7021-read-temp", HelpText = "Reads temperature from Si7021 sensor connected on I2C bus")]
    public class ReadTempFromSi7021 : I2cCommand, ICommandVerb
    {
        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateI2cDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of an <see cref="I2cDevice"/> implementation:
        ///     <code>using (var i2c = new Windows10I2cDevice(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            const int si7021_device_address = 0x40;
            var connectionSettings = new I2cConnectionSettings(BusId, si7021_device_address);

            using (var i2c = CreateI2cDevice(connectionSettings))
            {
                const byte temperatureCommand = 0xE3;
                var buffer = new byte[2];

                // Send temperature command, read back two bytes
                i2c.WriteByte(temperatureCommand);
                i2c.Read(buffer.AsSpan());

                // Calculate temperature
                var temp_code = buffer[0] << 8 | buffer[1];
                var temp_celcius = (((175.72 * temp_code) / (float)65536) - 46.85);
                var temp_fahrenheit = (temp_celcius * (9 / 5)) + 32;
                Console.WriteLine($"Temperature in fahrenheit: {temp_fahrenheit}");
                Console.WriteLine($"Temperature in celcius: {temp_celcius}");
            }

            return 0;
        }
    }
}
