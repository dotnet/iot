// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
    [Trait("feature", "gpio")]
    [Trait("feature", "gpio-libgpiod")]
    [Trait("SkipOnTestRun", "Windows_NT")]
    public class LibGpiodDriverTests : GpioControllerTestBase
    {
        public LibGpiodDriverTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override GpioDriver GetTestDriver() => new LibGpiodDriver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

        [Fact]
        public void SetPinModeSetsDefaultValue()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
    }
}
