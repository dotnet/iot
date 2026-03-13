// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

[Trait("feature", "gpio")]
[Trait("feature", "gpio-libgpiod2")]
[Trait("SkipOnTestRun", "Windows_NT")]
public class LibGpiodV2DriverTests : GpioControllerTestBase
{
    private const int ChipNumber = 0;

    /// <summary>
    /// Leave this pin open (unconnected) for the tests
    /// </summary>
    private const int OpenPin = 23;

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

            var otherInfo = ctrl.QueryComponentInformation();
            Assert.NotNull(otherInfo);
            Logger.WriteLine(otherInfo.ToString());
            ctrl.Dispose();
        }
    }

    /// <summary>
    /// This uses the RPI3 driver to verify that the libgpiod driver did what it was expected to do
    /// by directly reading out the hardware registers.
    /// </summary>
    private void VerifyStateUsingRpiDriver(int pin, PinMode expectedMode)
    {
        try
        {
            using var rpi = new GpioController(new RaspberryPi3Driver());
            rpi.OpenPin(pin);
            var mode = rpi.GetPinMode(pin);
            Assert.Equal(expectedMode, mode);
        }
        catch (PlatformNotSupportedException x)
        {
            Logger.WriteLine($"Unable to compare with RaspberryPi driver, as not a supported board type: {x.Message}");
        }
    }

    /// <summary>
    /// Tests for setting the pull up/pull down resistors on the Raspberry Pi (supported on Pi3 and Pi4, but with different techniques)
    /// </summary>
    [Fact]
    public void InputPullResistorsWork()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            // Verify all states
            controller.OpenPin(OpenPin, PinMode.Input);
            VerifyStateUsingRpiDriver(OpenPin, PinMode.Input);
            controller.ClosePin(OpenPin);

            controller.OpenPin(OpenPin, PinMode.InputPullUp);
            Thread.Sleep(50);
            VerifyStateUsingRpiDriver(OpenPin, PinMode.InputPullUp);
            Assert.Equal(PinValue.High, controller.Read(OpenPin));

            for (int i = 0; i < 100; i++)
            {
                controller.SetPinMode(OpenPin, PinMode.Input);
                VerifyStateUsingRpiDriver(OpenPin, PinMode.Input);
                controller.SetPinMode(OpenPin, PinMode.InputPullDown);
                Thread.Sleep(10);
                VerifyStateUsingRpiDriver(OpenPin, PinMode.InputPullDown);
                Assert.Equal(PinValue.Low, controller.Read(OpenPin));

                controller.SetPinMode(OpenPin, PinMode.InputPullUp);
                Thread.Sleep(10);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
            }

            // change one more time so that when running test in a loop we start with the inverted option
            controller.SetPinMode(OpenPin, PinMode.InputPullDown);
            Assert.Equal(PinValue.Low, controller.Read(OpenPin));
        }
    }

    protected override GpioDriver GetTestDriver() => new LibGpiodV2Driver(ChipNumber);
}
