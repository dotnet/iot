// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Threading;
using Iot.Device.DCMotor.Tests;
using Xunit;

namespace Iot.Device.ExplorerHat.Tests
{
    public class ExplorerHatTests
    {
        // Motor 1 (speed, direction), motor 2 (speed, direction) and the four lights
        private static readonly int[] OutputPins = { 19, 20, 21, 26, 4, 17, 27, 5 };

        [Fact]
        public void DisposeWithTheMotorsRunningLeavesEveryPinLow()
        {
            FakeGpioDriver driver = new();
            ExplorerHat hat = new(new GpioController(driver), shouldDispose: true);
            hat.Lights.On();
            hat.Motors.One.Speed = -0.5;
            hat.Motors.Two.Speed = 0.8;
            Thread.Sleep(100);

            hat.Dispose();

            Assert.True(driver.IsDisposed);
            foreach (int pin in OutputPins)
            {
                Assert.Equal(PinValue.Low, driver.GetValue(pin));
            }
        }

        [Fact]
        public void DisposeDoesNotDisposeACallerOwnedController()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            ExplorerHat hat = new(controller, shouldDispose: false);
            hat.Motors.Forwards(0.5);
            Thread.Sleep(100);

            hat.Dispose();

            Assert.False(driver.IsDisposed);
            controller.OpenPin(16, PinMode.Output);
            controller.Write(16, PinValue.High);
            Assert.Equal(PinValue.High, driver.GetValue(16));
        }
    }
}
