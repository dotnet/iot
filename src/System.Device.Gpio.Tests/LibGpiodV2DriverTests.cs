// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
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
            ctrl.Dispose();
        }
    }

    private static void VerifyStateUsingRpiDriver(int pin, PinMode expectedMode)
    {
        using var rpi = new GpioController(new RaspberryPi3Driver());
        rpi.OpenPin(pin);
        var mode = rpi.GetPinMode(pin);
        Assert.Equal(expectedMode, mode);
    }

    /// <summary>
    /// Tests for setting the pull up/pull down resistors on the Raspberry Pi (supported on Pi3 and Pi4, but with different techniques)
    /// </summary>
    [Fact]
    public void InputPullResistorsWork()
    {
        int j = 20;
        while (!Debugger.IsAttached && j-- > 0)
        {
            Logger.WriteLine("Waiting for debugger");
            Thread.Sleep(1000);
        }

        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OpenPin, PinMode.Input);
            Thread.Sleep(1000);
            VerifyStateUsingRpiDriver(OpenPin, PinMode.Input);
            controller.ClosePin(OpenPin);

            controller.OpenPin(OpenPin, PinMode.InputPullUp);
            Thread.Sleep(100);
            VerifyStateUsingRpiDriver(OpenPin, PinMode.InputPullUp);
            Assert.Equal(PinValue.High, controller.Read(OpenPin));

            for (int i = 0; i < 100; i++)
            {
                Logger.WriteLine($"Starting iteration {i}");
                controller.SetPinMode(OpenPin, PinMode.InputPullDown);
                Thread.Sleep(100);
                VerifyStateUsingRpiDriver(OpenPin, PinMode.InputPullDown);
                Assert.Equal(PinValue.Low, controller.Read(OpenPin));

                controller.SetPinMode(OpenPin, PinMode.InputPullUp);
                Thread.Sleep(100);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
            }

            // change one more time so that when running test in a loop we start with the inverted option
            controller.SetPinMode(OpenPin, PinMode.InputPullDown);
            Assert.Equal(PinValue.Low, controller.Read(OpenPin));
        }
    }

#pragma warning disable SDGPIO0001
    protected override GpioDriver GetTestDriver() => new LibGpiodV2Driver(ChipNumber);
}
