// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

[Trait("requirement", "root")]
[Trait("feature", "gpio")]
[Trait("feature", "gpio-rpi3")]
public class RaspberryPiDriverTests : GpioControllerTestBase
{
    /// <summary>
    /// Leave this pin open (unconnected) for the tests
    /// </summary>
    private const int OpenPin = 23;
    public RaspberryPiDriverTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override GpioDriver GetTestDriver() => new RaspberryPi3Driver();

    /// <summary>
    /// Tests for setting the pull up/pull down resistors on the Raspberry Pi (supported on Pi3 and Pi4, but with different techniques)
    /// </summary>
    [Fact]
    public void InputPullResistorsWork()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OpenPin, PinMode.InputPullUp);
            Assert.Equal(PinValue.High, controller.Read(OpenPin));

            for (int i = 0; i < 100; i++)
            {
                controller.SetPinMode(OpenPin, PinMode.InputPullDown);
                Assert.Equal(PinValue.Low, controller.Read(OpenPin));

                controller.SetPinMode(OpenPin, PinMode.InputPullUp);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
            }

            // change one more time so that when running test in a loop we start with the inverted option
            controller.SetPinMode(OpenPin, PinMode.InputPullDown);
            Assert.Equal(PinValue.Low, controller.Read(OpenPin));
        }
    }

    [Fact]
    public void OpenPinDefaultsModeToLastModeIncludingPulls()
    {
        // This is only fully supported on the Pi4
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OutputPin);
            controller.SetPinMode(OutputPin, PinMode.InputPullDown);
            controller.ClosePin(OutputPin);
            controller.OpenPin(OutputPin);
            if (IsRaspi4())
            {
                Assert.Equal(PinMode.InputPullDown, controller.GetPinMode(OutputPin));
            }
            else
            {
                Assert.Equal(PinMode.Input, controller.GetPinMode(OutputPin));
            }
        }
    }

    [Fact]
    public void HighPulledPinDoesNotChangeToLowWhenChangedToOutput()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            bool didTriggerToLow = false;
            int testPin = OutputPin;
            // Set value to low prior to test, so that we have a defined start situation
            controller.OpenPin(testPin, PinMode.Output);
            controller.Write(testPin, PinValue.Low);
            controller.ClosePin(testPin);
            // For this test, we use the input pin as an external pull-up
            controller.OpenPin(InputPin, PinMode.Output);
            controller.Write(InputPin, PinValue.High);
            Thread.Sleep(2);
            // If we were to use InputPullup here, this would work around the problem it seems, but it would also make our test pass under almost all situations
            controller.OpenPin(testPin, PinMode.Input);
            Thread.Sleep(50);
            controller.RegisterCallbackForPinValueChangedEvent(testPin, PinEventTypes.Falling, (sender, args) =>
            {
                if (args.ChangeType == PinEventTypes.Falling)
                {
                    didTriggerToLow = true;
                }
            });

            controller.Write(testPin, PinValue.High);
            controller.SetPinMode(testPin, PinMode.Output);
            Thread.Sleep(50);
            Assert.False(didTriggerToLow);

            controller.ClosePin(OutputPin);
            controller.ClosePin(InputPin);
        }
    }
}
