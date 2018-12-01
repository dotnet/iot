// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    public enum GpioDriverType
    {
        [ImplementationType(null)]
        Default,

        [ImplementationType(typeof(Windows10Driver))]
        Windows,

        [ImplementationType(typeof(UnixDriver))]
        Unix,

        [ImplementationType(typeof(HummingBoardDriver))]
        HummingBoard,

        [ImplementationType(typeof(RaspberryPi3Driver))]
        RPi3,
    }
}
