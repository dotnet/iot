// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateSpiDevice"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of a <see cref="SpiDevice"/> implementation:
        ///     <code>using (var spi = SpiDevice.Create(connectionSettings))</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"ByteCount={ByteCount}, BusId={BusId}, ChipSelectLine={ChipSelectLine}, Mode={Mode}, DataBitLength={DataBitLength}, ClockFrequency={ClockFrequency}, Device={Device}");

            var connectionSettings = new SpiConnectionSettings(BusId, ChipSelectLine)
            {
                ClockFrequency = ClockFrequency,
                DataBitLength = DataBitLength,
                Mode = Mode,
            };

            using (var spiDevice = CreateSpiDevice(connectionSettings))
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
