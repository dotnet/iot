// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable SDGPIO0001
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

#pragma warning disable SDGPIO0001
    protected override GpioDriver GetTestDriver() => new LibGpiodDriver(ChipNumber, LibGpiodDriverVersion.V2);
}
