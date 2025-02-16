// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

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
        var chips = SysFsDriver.GetAvailableChips();
        Assert.NotEmpty(chips);
        if (IsRaspi4())
        {
            // 2 real ones and the default 0 entry
            Assert.Equal(3, chips.Count);
        }

        foreach (var chip in chips)
        {
            var driver = new SysFsDriver(chip.Id);
            var ctrl = new GpioController(driver);
            Assert.NotNull(ctrl);
            var driverInfo = driver.GetChipInfo();
            Assert.NotNull(driverInfo);
            Assert.Equal(chip, driverInfo);
            ctrl.Dispose();
        }
    }

    private bool IsRaspi4()
    {
        if (File.Exists("/proc/device-tree/model"))
        {
            string model = File.ReadAllText("/proc/device-tree/model", Text.Encoding.ASCII);
            if (model.Contains("Raspberry Pi 4") || model.Contains("Raspberry Pi Compute Module 4"))
            {
                return true;
            }
        }

        return false;
    }
}
