// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using System.Threading;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public class RaspberryPiComputeModuleDriverTests : GpioControllerTestBase
    {
        private const int GpioBank1OutputPin = 37;
        private const int GpioBank1InputPin = 34;
        protected override GpioDriver GetTestDriver() => new RaspberryPiComputeModule3Driver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

        [Fact]
        [Trait("SkipOnTestRun", "Windows_NT")] //not sure if Win 10 IoT supports CM3 yet, need to confirm
        public void GpioBank1PinValueReadAndWrite()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(GpioBank1OutputPin, PinMode.Output);
                controller.OpenPin(GpioBank1InputPin, PinMode.Input);
                controller.Write(GpioBank1OutputPin, PinValue.High);
                Thread.SpinWait(100);
                Assert.Equal(PinValue.High, controller.Read(GpioBank1InputPin));
                controller.Write(GpioBank1OutputPin, PinValue.Low);
                Thread.SpinWait(100);
                Assert.Equal(PinValue.Low, controller.Read(GpioBank1InputPin));
                controller.Write(GpioBank1OutputPin, PinValue.High);
                Thread.SpinWait(100);
                Assert.Equal(PinValue.High, controller.Read(GpioBank1InputPin));
            }
        }
    }
}
