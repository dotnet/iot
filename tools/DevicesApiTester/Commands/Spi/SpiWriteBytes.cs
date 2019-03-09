// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    [Verb("spi-write-bytes", HelpText = "Converts a hexadecimal string to byte array and writes to a specified SPI channel.")]
    public class SpiWriteBytes : SpiCommand, ICommandVerb
    {
        [Option('h', "hex-string", HelpText = "The hexadecimal string to convert and write.  Each byte in string must be represented by two hexadecimal characters.", Required = true)]
        public string HexString { get; set; }

        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateSpiDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of a <see cref="SpiDevice"/> implementation:
        ///     <code>using (var spi = new UnixSpiDevice(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"Device={Device}, BusId={BusId}, ChipSelectLine={ChipSelectLine}, Mode={Mode}, DataBitLength={DataBitLength}, ClockFrequency={ClockFrequency}, HexString={HexString}");

            var connectionSettings = new SpiConnectionSettings(BusId, ChipSelectLine)
            {
                ClockFrequency = ClockFrequency,
                DataBitLength = DataBitLength,
                Mode = Mode,
            };

            using (var spiDevice = CreateSpiDevice(connectionSettings))
            {
                // This will verify value as in hexadecimal.
                var writeBuffer = HexStringUtilities.HexStringToByteArray(HexString);
                spiDevice.Write(writeBuffer.AsSpan());
            }

            return 0;
        }
    }
}
