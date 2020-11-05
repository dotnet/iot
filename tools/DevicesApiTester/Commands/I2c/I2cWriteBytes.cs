// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    [Verb("i2c-write-bytes", HelpText = "Converts a hexadecimal string to byte array and writes to a specified I2C device.")]
    public class I2cWriteBytes : I2cCommand, ICommandVerb
    {
        [Option('a', "device-address", HelpText = "The bus address of the device.", Required = true)]
        public int DeviceAddress { get; set; }

        [Option('h', "hex-string", HelpText = "The hexadecimal string to convert and write.  Each byte in string must be represented by two hexadecimal characters.", Required = true)]
        public string? HexString { get; set; }

        /// <summary>Executes the command.</summary>
        /// <returns>The command's exit code.</returns>
        public int Execute()
        {
            Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress} (0x{DeviceAddress:X2}), HexString={HexString}");

            if (HexString is null)
            {
                throw new Exception($"{nameof(HexString)} is null");
            }

            var connectionSettings = new I2cConnectionSettings(BusId, DeviceAddress);

            using (var i2cDevice = I2cDevice.Create(connectionSettings))
            {
                // This will verify value as in hexadecimal.
                var writeBuffer = HexStringUtilities.HexStringToByteArray(HexString);
                i2cDevice.Write(writeBuffer.AsSpan());
            }

            return 0;
        }
    }
}
