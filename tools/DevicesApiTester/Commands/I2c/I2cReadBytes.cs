// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-read-bytes", HelpText = "Reads bytes from a specified I2C device.")]
    public class I2cReadBytes : I2cCommand, ICommandVerb
    {
        [Option('a', "device-address", HelpText = "The bus address of the device.", Required = true)]
        public int DeviceAddress { get; set; }

        [Option('f', "first-register", HelpText = "Beginning register address to read.", Required = false)]
        public int? FirstRegister { get; set; }

        [Option('n', "byte-count", HelpText = "The number of bytes to read.", Required = false, Default = 16)]
        public int ByteCount { get; set; }

        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        public int Execute()
        {
            if (FirstRegister == null)
            {
                Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress} (0x{DeviceAddress:X2}), ByteCount={ByteCount}");
            }
            else
            {
                Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress} (0x{DeviceAddress:X2}), FirstRegister={ FirstRegister} (0x{FirstRegister:X2}), ByteCount={ByteCount}");
            }

            var connectionSettings = new I2cConnectionSettings(BusId, DeviceAddress);

            using (var i2cDevice = I2cDevice.Create(connectionSettings))
            {
                // Set to first address if needed.
                if (FirstRegister != null)
                {
                    i2cDevice.WriteByte((byte)FirstRegister);
                }

                // Read bytes of data.
                var buffer = new byte[ByteCount];
                i2cDevice.Read(buffer.AsSpan());

                Console.WriteLine($"Bytes Read:{Environment.NewLine}{HexStringUtilities.FormatByteData(buffer)}");
            }

            return 0;
        }
    }
}
