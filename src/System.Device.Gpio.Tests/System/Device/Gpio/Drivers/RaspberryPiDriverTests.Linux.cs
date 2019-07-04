// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using System.Device.Gpio.Tests;
using Xunit;

namespace System.Device.Gpio.Drivers.Tests
{
    public class RaspberryPiDriverTests : GpioControllerTestBase
    {
        protected override GpioDriver GetTestDriver() => new RaspberryPi3Driver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
    }
}
