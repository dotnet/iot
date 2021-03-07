// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
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

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

        /// <summary>
        /// Tests for setting the pull up/pull down resistors on the Raspberry Pi (supported on Pi3 and Pi4, but with different techniques)
        /// </summary>
        [Fact]
        public void InputPullResistorsWork()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
        public void HighPulledPinDoesNotGlitchToLowWhenChangedToOutput()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                bool didTriggerToLow = false;
                int testPin = OutputPin; // TODO: Change to OpenPin
                controller.OpenPin(testPin, PinMode.InputPullUp);
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
            }
        }
    }
}
