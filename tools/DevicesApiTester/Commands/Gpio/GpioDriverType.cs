// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    public enum GpioDriverType
    {
        [ImplementationType(null)]
        Default,

        [ImplementationType(typeof(UnixDriver))]
        Unix,

        [ImplementationType(typeof(RaspberryPi3Driver))]
        RPi3,
    }
}
