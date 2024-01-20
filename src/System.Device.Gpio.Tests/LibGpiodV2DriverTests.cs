// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

[Trait("feature", "gpio")]
[Trait("feature", "gpio-libgpiod2")]
[Trait("SkipOnTestRun", "Windows_NT")]
public class LibGpiodV2DriverTests : GpioControllerTestBase
{
    private const int ChipNumber = 0;

    public LibGpiodV2DriverTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override GpioDriver GetTestDriver() => new LibGpiodDriver(ChipNumber, LibGpiodDriverVersion.V2);

    protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
}
