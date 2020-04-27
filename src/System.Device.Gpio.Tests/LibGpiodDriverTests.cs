// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
    [Trait("feature", "gpio")]
    [Trait("feature", "gpio-libgpiod")]
    [Trait("SkipOnTestRun", "Windows_NT")]
    public class LibGpiodDriverTests : GpioControllerTestBase
    {
        public LibGpiodDriverTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override GpioDriver GetTestDriver() => new LibGpiodDriver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
    }
}
