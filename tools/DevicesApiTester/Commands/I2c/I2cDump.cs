// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Devices;
using System.Text;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-dump", HelpText = "Examines registers of specified I2C device.")]
    public class I2cDump : I2cCommand, ICommandVerb
    {
        [Option('a', "device-address", HelpText = "The bus address of the device.", Required = true)]
        public int DeviceAddress { get; set; }

        [Option('f', "first-register", HelpText = "Beginning register address in range to read.", Required = false, Default = 0x00)]
        public int FirstRegister { get; set; }

        [Option('l', "last-register", HelpText = "Last register address in range to read.", Required = false, Default = 0xFF)]
        public int LastRegister { get; set; }

        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateI2cDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of an <see cref="I2cDevice"/> implementation:
        ///     <code>using (var i2cDevice = I2cDevice.Create(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"Device={Device}, BusId={BusId}, DeviceAddress={DeviceAddress} (0x{DeviceAddress:X2}), FirstRegister={FirstRegister} (0x{FirstRegister:X2}), LastRegister={LastRegister} (0x{LastRegister:X2})");

            if (FirstRegister > FirstRegister)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(FirstRegister),
                    $"The first register address ({FirstRegister}) is larger than the last register address ({FirstRegister}) in range to read.");
            }

            ReadDeviceRegisters();
            return 0;
        }

        private void ReadDeviceRegisters()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");
            stringBuilder.Append(Environment.NewLine);

            var connectionSettings = new I2cConnectionSettings(BusId, DeviceAddress);
            using (var i2cDevice = I2cDevice.Create(connectionSettings))
            {
                for (int startingRowAddress = 0; startingRowAddress < 255; startingRowAddress += 16)
                {
                    stringBuilder.Append($"{startingRowAddress:x2}: ");  // Beginning of row.

                    for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                    {
                        int registerAddress = startingRowAddress + rowAddress;

                        // Skip the unwanted addresses.
                        if (registerAddress < FirstRegister || registerAddress > LastRegister)
                        {
                            stringBuilder.Append("   ");
                            continue;
                        }

                        i2cDevice.WriteByte((byte)registerAddress);
                        byte data = i2cDevice.ReadByte();
                        stringBuilder.Append($"{data:x2} ");
                    }

                    stringBuilder.Append(Environment.NewLine);
                }
            }

            Console.WriteLine(stringBuilder.ToString());
        }
    }
}
