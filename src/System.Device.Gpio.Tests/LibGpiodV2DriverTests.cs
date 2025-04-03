// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
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

    [Fact]
    public void CheckAllChipsCanBeConstructed()
    {
        var chips = LibGpiodV2Driver.GetAvailableChips();
        foreach (var c in chips)
        {
            Logger.WriteLine(c.ToString());
        }

        Assert.NotEmpty(chips);
        if (IsRaspi4())
        {
            // 3 real ones and the default 0 entry
            Assert.Equal(3, chips.Count);
        }

        foreach (var chip in chips)
        {
            var driver = new LibGpiodV2Driver(chip.Id);
            var ctrl = new GpioController(driver);
            Assert.NotNull(ctrl);
            var driverInfo = driver.GetChipInfo();
            Assert.NotNull(driverInfo);
            Assert.Equal(chip, driverInfo);
            ctrl.Dispose();
        }
    }

#pragma warning disable SDGPIO0001
    protected override GpioDriver GetTestDriver() => new LibGpiodV2Driver(ChipNumber);
}
