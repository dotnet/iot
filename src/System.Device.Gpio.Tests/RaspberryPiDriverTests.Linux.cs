// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public class RaspberryPiDriverTests : GpioControllerTestBase
    {
        /// <summary>
        /// Leave this pin open (unconnected) for the tests
        /// </summary>
        private const int OpenPin = 23;

        protected override GpioDriver GetTestDriver() => new RaspberryPi3Driver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

        /// <summary>
        /// Tests for setting the pull up/pull down resistors on the Raspberry Pi (supported on Pi3 and Pi4, but with different techniques)
        /// </summary>
        [Fact]
        public void InputPullResistorsWork()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme()))
            {
                controller.OpenPin(OpenPin, PinMode.InputPullUp);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
                controller.SetPinMode(OpenPin, PinMode.InputPullDown);
                Assert.Equal(PinValue.Low, controller.Read(OpenPin));
                controller.SetPinMode(OpenPin, PinMode.InputPullUp);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
            }
        }
    }
}
