// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
    [Trait("requirement", "root")]
    [Trait("feature", "gpio")]
    [Trait("feature", "gpio-sysfs")]
    [Trait("SkipOnTestRun", "Windows_NT")]
    public class SysFsDriverTests : GpioControllerTestBase
    {
        public SysFsDriverTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override GpioDriver GetTestDriver() => new SysFsDriver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
    }
}
