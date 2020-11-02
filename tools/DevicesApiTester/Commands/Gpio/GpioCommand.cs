// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    public abstract class GpioCommand : DebuggableCommand
    {
        [Option('s', "scheme", HelpText = "The pin numbering scheme: { Logical | Board }", Required = false, Default = PinNumberingScheme.Logical)]
        public PinNumberingScheme Scheme { get; set; }

        [Option('d', "driver", HelpText = "The GpioDriver to use: { Default | Windows | UnixSysFs | HummingBoard | RPi3 }", Required = false, Default = GpioDriverType.Default)]
        public GpioDriverType Driver { get; set; }

        protected GpioController CreateGpioController()
        {
            GpioDriver gpioDriver = DriverFactory.CreateFromEnum<GpioDriver, GpioDriverType>(Driver);

            return gpioDriver != null
                ? new GpioController(Scheme, gpioDriver)
                : new GpioController(Scheme);
        }
    }
}
