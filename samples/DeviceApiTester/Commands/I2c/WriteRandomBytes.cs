// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-write-random-bytes", HelpText = "Writes random bytes to a specified I2C channel.")]
    public class WriteRandomBytes : I2cCommand, ICommandVerb
    {
        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateI2cDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of an <see cref="I2cDevice"/> implementation:
        ///     <code>using (var i2c = new UnixI2cDevice(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"ByteCount={ByteCount}, BusId={BusId}, DeviceAddress={DeviceAddress}, Device={Device}");

            var connectionSettings = new I2cConnectionSettings(BusId, DeviceAddress);

            using (var i2c = CreateI2cDevice(connectionSettings))
            {
                // Write random bytes of data
                var buffer = new byte[ByteCount];
                new Random().NextBytes(buffer);

                Console.WriteLine($"Writing random bytes:{Environment.NewLine}{HexStringUtilities.FormatByteData(buffer)}");
                i2c.Write(buffer.AsSpan());
            }

            return 0;
        }

        [Option('n', "byte-count", HelpText = "The number of random bytes to write to the connection", Required = false, Default = 16)]
        public int ByteCount { get; set; }
    }
}
