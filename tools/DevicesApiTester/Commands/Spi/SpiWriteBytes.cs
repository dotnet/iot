// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public int Execute()
        {
            Console.WriteLine($"BusId={BusId}, ChipSelectLine={ChipSelectLine}, Mode={Mode}, DataBitLength={DataBitLength}, ClockFrequency={ClockFrequency}, HexString={HexString}");

            var connectionSettings = new SpiConnectionSettings(BusId, ChipSelectLine)
            {
                ClockFrequency = ClockFrequency,
                DataBitLength = DataBitLength,
                Mode = Mode,
            };

            using (var spiDevice = SpiDevice.Create(connectionSettings))
            {
                // This will verify value as in hexadecimal.
                var writeBuffer = HexStringUtilities.HexStringToByteArray(HexString);
                spiDevice.Write(writeBuffer.AsSpan());
            }

            return 0;
        }
    }
}
