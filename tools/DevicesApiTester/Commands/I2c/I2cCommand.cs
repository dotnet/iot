// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    public abstract class I2cCommand : DebuggableCommand
    {
        [Option('d', "device", HelpText = "The I2cDevice to use: { Windows | Unix }", Required = true)]
        public I2cDriverType Device { get; set; }

        [Option('b', "bus-id", HelpText = "The bus ID the device is connected to.", Required = true)]
        public int BusId { get; set; }

        protected I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            return DriverFactory.CreateFromEnum<I2cDevice, I2cDriverType>(this.Device, connectionSettings);
        }
    }
}
