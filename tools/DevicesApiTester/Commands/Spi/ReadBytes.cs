// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    [Verb("spi-read-bytes", HelpText = "Reads bytes from a specified SPI channel.")]
    public class ReadBytes : SpiCommand, ICommandVerb
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
                // Read bytes of data
                var buffer = new byte[ByteCount];
                spiDevice.Read(buffer.AsSpan());

                Console.WriteLine($"Bytes read:{Environment.NewLine}{HexStringUtilities.FormatByteData(buffer)}");
            }

            return 0;
        }

        [Option('n', "byte-count", HelpText = "The number of bytes to read from the connection", Required = false, Default = 16)]
        public int ByteCount { get; set; }
    }
}
