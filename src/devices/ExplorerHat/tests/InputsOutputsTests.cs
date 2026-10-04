// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Linq;
using Iot.Device.DCMotor.Tests;
using Xunit;

namespace Iot.Device.ExplorerHat.Tests
{
    public class InputsOutputsTests
    {
        private static readonly int[] InputPins = { 23, 22, 24, 25 };
        private static readonly int[] OutputPins = { 6, 12, 13, 16 };

        [Fact]
        public void InputsAndOutputsUseTheExplorerHatPins()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            Assert.Equal(InputPins, hat.Inputs.Select(input => input.Pin));
            Assert.Equal(OutputPins, hat.Outputs.Select(output => output.Pin));
            Assert.Equal(new[] { 23, 22, 24, 25 }, new[] { hat.Inputs.One.Pin, hat.Inputs.Two.Pin, hat.Inputs.Three.Pin, hat.Inputs.Four.Pin });
            Assert.Equal(new[] { 6, 12, 13, 16 }, new[] { hat.Outputs.One.Pin, hat.Outputs.Two.Pin, hat.Outputs.Three.Pin, hat.Outputs.Four.Pin });
        }

        [Fact]
        public void NoInputOrOutputPinIsOpenedUntilItIsUsed()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            foreach (int pin in InputPins.Concat(OutputPins))
            {
                Assert.False(controller.IsPinOpen(pin));
            }
        }

        [Fact]
        public void ReadOpensThePinAsInputAndReturnsItsValue()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            using ExplorerHat hat = new(controller, shouldDispose: false);

            driver.SetInputValue(25, PinValue.High);
            Assert.Equal(PinValue.High, hat.Inputs.Four.Read());
            Assert.True(controller.IsPinOpen(25));
            Assert.Equal(PinMode.Input, controller.GetPinMode(25));

            driver.SetInputValue(25, PinValue.Low);
            Assert.Equal(PinValue.Low, hat.Inputs.Four.Read());
            Assert.False(controller.IsPinOpen(23));
        }

        [Fact]
        public void OnAndOffOpenThePinAsOutputAndWriteIt()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            using ExplorerHat hat = new(controller, shouldDispose: false);

            hat.Outputs.Four.On();
            Assert.True(hat.Outputs.Four.IsOn);
            Assert.Equal(PinValue.High, driver.GetValue(16));
            Assert.Equal(PinMode.Output, controller.GetPinMode(16));

            hat.Outputs.Four.Off();
            Assert.False(hat.Outputs.Four.IsOn);
            Assert.Equal(PinValue.Low, driver.GetValue(16));

            hat.Outputs.One.Off();
            Assert.True(controller.IsPinOpen(6));
            Assert.Equal(PinValue.Low, driver.GetValue(6));
        }

        [Fact]
        public void DisposeSwitchesOffTheOutputsAndClosesOnlyThePinsItOpened()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            ExplorerHat hat = new(controller, shouldDispose: false);

            // Output 1 is used by the caller with the same controller (for example, the trigger of a distance sensor)
            controller.OpenPin(6, PinMode.Output);
            controller.Write(6, PinValue.High);
            hat.Outputs.Four.On();
            hat.Inputs.Two.Read();

            hat.Dispose();

            Assert.Equal(PinValue.Low, driver.GetValue(16));
            Assert.False(hat.Outputs.Four.IsOn);
            Assert.False(controller.IsPinOpen(16));
            Assert.False(controller.IsPinOpen(22));
            Assert.True(controller.IsPinOpen(6));
            Assert.Equal(PinValue.High, driver.GetValue(6));
        }

        [Fact]
        public void UsingAnInputOrOutputAfterDisposeThrows()
        {
            using GpioController controller = new(new FakeGpioDriver());
            ExplorerHat hat = new(controller, shouldDispose: false);
            hat.Dispose();

            Assert.Throws<ObjectDisposedException>(() => hat.Inputs.One.Read());
            Assert.Throws<ObjectDisposedException>(() => hat.Outputs.One.On());
            Assert.Throws<ObjectDisposedException>(() => hat.Outputs.One.Off());
        }
    }
}
