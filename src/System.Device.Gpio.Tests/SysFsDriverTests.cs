// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable SDGPIO0001
namespace System.Device.Gpio.Tests;

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

    [Fact]
    public void CheckAllChipsCanBeConstructed()
    {
        string[] fileNames = Directory.GetFileSystemEntries("/sys/class/gpio", $"*", SearchOption.TopDirectoryOnly);
        Logger.WriteLine("Content of /sys/class/gpio:");
        foreach (var f in fileNames)
        {
            Logger.WriteLine(f);
        }

        var chips = SysFsDriver.GetAvailableChips();
        Logger.WriteLine("Available chips:");
        foreach (var c in chips)
        {
            Logger.WriteLine(c.ToString());
        }

        Assert.NotEmpty(chips);
        if (IsRaspi4())
        {
            // 2 real ones and the default 0 entry
            Assert.Equal(3, chips.Count);
        }

        foreach (var chip in chips)
        {
            var driver = new SysFsDriver(chip);
            var ctrl = new GpioController(driver);
            Assert.NotNull(ctrl);
            var driverInfo = driver.GetChipInfo();
            Assert.NotNull(driverInfo);
            Assert.Equal(chip, driverInfo);
            ctrl.Dispose();
        }
    }
}
