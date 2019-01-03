// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Text;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-detect", HelpText = "Scans and lists detected devices from a specified I2C bus.")]
    public class I2cDetect : I2cCommand, ICommandVerb
    {
        [Option('f', "first-address", HelpText = "Beginning device address in range to scan.", Required = false, Default = 0x03)]
        public int FirstAddress { get; set; }

        [Option('l', "last-address", HelpText = "Last device address in range to scan.", Required = false, Default = 0x77)]
        public int LastAddress { get; set; }

        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateI2cDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of an <see cref="I2cDevice"/> implementation:
        ///     <code>using (var i2cDevice = new UnixI2cDevice(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"Device={Device}, BusId={BusId}, FirstAddress={FirstAddress} (0x{FirstAddress:X2}), LastAddress={LastAddress} (0x{LastAddress:X2})");

            if (FirstAddress > LastAddress)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(FirstAddress),
                    $"The first device address ({FirstAddress}) is larger than the last device address ({LastAddress}) in range to scan.");
            }

            ScanDeviceAddressesOnI2cBus();
            return 0;
        }

        private void ScanDeviceAddressesOnI2cBus()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");
            stringBuilder.Append(Environment.NewLine);

            for (int startingRowAddress = 0; startingRowAddress < 128; startingRowAddress += 16)
            {
                stringBuilder.Append($"{startingRowAddress:x2}: ");  // Beginning of row.

                for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                {
                    int deviceAddress = startingRowAddress + rowAddress;

                    // Skip the unwanted addresses.
                    if (deviceAddress < FirstAddress || deviceAddress > LastAddress)
                    {
                        stringBuilder.Append("   ");
                        continue;
                    }

                    var connectionSettings = new I2cConnectionSettings(BusId, deviceAddress);
                    using (var i2cDevice = CreateI2cDevice(connectionSettings))
                    {
                        try
                        {
                            i2cDevice.ReadByte();  // Only checking if device is present.
                            stringBuilder.Append($"{deviceAddress:x2} ");
                        }
                        catch
                        {
                            stringBuilder.Append("-- ");
                        }
                    }
                }

                stringBuilder.Append(Environment.NewLine);
            }

            Console.WriteLine(stringBuilder.ToString());
        }
    }
}
