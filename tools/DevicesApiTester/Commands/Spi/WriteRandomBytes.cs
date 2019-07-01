// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    [Verb("spi-write-random-bytes", HelpText = "Writes random bytes to a specified SPI channel.")]
    public class WriteRandomBytes : SpiCommand, ICommandVerb
    {
        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        public int Execute()
        {
            Console.WriteLine($"ByteCount={ByteCount}, BusId={BusId}, ChipSelectLine={ChipSelectLine}, Mode={Mode}, DataBitLength={DataBitLength}, ClockFrequency={ClockFrequency}");

            var connectionSettings = new SpiConnectionSettings(BusId, ChipSelectLine)
            {
                ClockFrequency = ClockFrequency,
                DataBitLength = DataBitLength,
                Mode = Mode,
            };

            using (var spiDevice = SpiDevice.Create(connectionSettings))
            {
                // Write random bytes of data
                var buffer = new byte[ByteCount];
                new Random().NextBytes(buffer);

                Console.WriteLine($"Writing random bytes:{Environment.NewLine}{HexStringUtilities.FormatByteData(buffer)}");
                spiDevice.Write(buffer.AsSpan());
            }

            return 0;
        }

        [Option('n', "byte-count", HelpText = "The number of random bytes to write to the connection", Required = false, Default = 16)]
        public int ByteCount { get; set; }
    }
}
