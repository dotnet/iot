// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
    [Trait("SkipOnTestRun", "Unix")]
    public class WindowsDriverTests : GpioControllerTestBase
    {
        public WindowsDriverTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override GpioDriver GetTestDriver() => new Windows10Driver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
    }
}
