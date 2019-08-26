// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    public abstract class I2cCommand : DebuggableCommand
    {
        [Option('b', "bus-id", HelpText = "The bus ID the device is connected to.", Required = true)]
        public int BusId { get; set; }
    }
}
