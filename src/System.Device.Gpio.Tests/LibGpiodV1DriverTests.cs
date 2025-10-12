// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

#pragma warning disable SDGPIO0001

[Trait("feature", "gpio")]
[Trait("feature", "gpio-libgpiod")]
[Trait("SkipOnTestRun", "Windows_NT")]
public class LibGpiodV1DriverTests : GpioControllerTestBase
{
    public LibGpiodV1DriverTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override GpioDriver GetTestDriver() => new LibGpiodDriver(0);

    [Fact]
    public void SetPinModeSetsDefaultValue()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            int testPin = OutputPin;
            // Set value to low prior to test, so that we have a defined start situation
            controller.OpenPin(testPin, PinMode.Output);
            controller.Write(testPin, PinValue.Low);
            controller.ClosePin(testPin);
            // For this test, we use the input pin as an external pull-up
            controller.OpenPin(InputPin, PinMode.Output);
            controller.Write(InputPin, PinValue.High);
            Thread.Sleep(2);

            controller.OpenPin(testPin, PinMode.Input);
            Thread.Sleep(50);
            // It's not possible to change the direction while listening to events (causes an error). Therefore the real behavior of the driver
            // can only be tested with a scope (or if we had a third pin connected in the lab hardware)

            // We do another test here and make sure the
            // pin is really high now
            controller.Write(testPin, PinValue.High);
            controller.SetPinMode(testPin, PinMode.Output);
            controller.SetPinMode(InputPin, PinMode.Input);

            Assert.True(controller.Read(InputPin) == PinValue.High);

            controller.ClosePin(OutputPin);
            controller.ClosePin(InputPin);
        }
    }

    [Fact]
    public void UnregisterPinValueChangedShallNotThrow()
    {
        using var gc = new GpioController(GetTestDriver());
        gc.OpenPin(InputPin, PinMode.Input);

        static void PinChanged(object sender, PinValueChangedEventArgs args)
        {
        }

        for (var i = 0; i < 1000; i++)
        {
            gc.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising | PinEventTypes.Falling, PinChanged);
            gc.UnregisterCallbackForPinValueChangedEvent(InputPin, PinChanged);
        }
    }

    /// <summary>
    /// Ensure leaking instances of the driver doesn't cause a segfault
    /// Regression test for https://github.com/dotnet/iot/issues/1849
    /// </summary>
    [Fact]
    public void LeakingDriverDoesNotCrash()
    {
        GpioController controller1 = new GpioController(new LibGpiodDriver());
        controller1.OpenPin(10, PinMode.Output);
        GpioController controller2 = new GpioController(new LibGpiodDriver());
        controller2.OpenPin(11, PinMode.Output);
        GpioController controller3 = new GpioController(new LibGpiodDriver());
        controller3.OpenPin(12, PinMode.Output);
        GpioController controller4 = new GpioController(new LibGpiodDriver());
        controller4.OpenPin(13, PinMode.Output);
        GpioController controller5 = new GpioController(new LibGpiodDriver());
        controller5.OpenPin(14, PinMode.Output);

        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GpioController controller6 = new GpioController(new LibGpiodDriver());
            controller6.OpenPin(15, PinMode.Output);
            controller6.ClosePin(15);
            controller6.Dispose();
            GC.Collect();
            Thread.Sleep(20);
        }

        GC.WaitForPendingFinalizers();
    }

    [Fact]
    public void CheckAllChipsCanBeConstructed()
    {
        var chips = LibGpiodDriver.GetAvailableChips();
        foreach (var c in chips)
        {
            Logger.WriteLine(c.ToString());
        }

        Assert.NotEmpty(chips);
        if (IsRaspi4())
        {
            // 2 entries. Here they're always named 0 and 1
            Assert.Equal(2, chips.Count);
        }

        foreach (var chip in chips)
        {
            var driver = new LibGpiodDriver(chip.Id);
            var ctrl = new GpioController(driver);
            Assert.NotNull(ctrl);
            var driverInfo = driver.GetChipInfo();
            Assert.NotNull(driverInfo);
            Assert.Equal(chip, driverInfo);
            ctrl.Dispose();
        }
    }
}
