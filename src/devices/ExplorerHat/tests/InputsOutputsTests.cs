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

            Assert.Equal(InputPins, hat.Inputs.Select(input => input.PinNumber));
            Assert.Equal(OutputPins, hat.Outputs.Select(output => output.PinNumber));
            Assert.Equal(new[] { 23, 22, 24, 25 }, new[] { hat.Inputs.One.PinNumber, hat.Inputs.Two.PinNumber, hat.Inputs.Three.PinNumber, hat.Inputs.Four.PinNumber });
            Assert.Equal(new[] { 6, 12, 13, 16 }, new[] { hat.Outputs.One.PinNumber, hat.Outputs.Two.PinNumber, hat.Outputs.Three.PinNumber, hat.Outputs.Four.PinNumber });
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
        public void InputsAndOutputsAreGpioPinsAndCanBeUsedByPosition()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            GpioPin input = hat.Inputs.Three;
            GpioPin output = hat.Outputs.Three;
            Assert.Equal(24, input.PinNumber);
            Assert.Equal(13, output.PinNumber);
            Assert.Equal(4, hat.Inputs.Count);
            Assert.Equal(4, hat.Outputs.Count);
            Assert.Same(hat.Inputs.One, hat.Inputs[0]);
            Assert.Same(hat.Inputs.Four, hat.Inputs[3]);
            Assert.Same(hat.Outputs.Two, hat.Outputs[1]);
            Assert.Equal(PinMode.Input, input.GetPinMode());
            Assert.Equal(PinMode.Output, output.GetPinMode());
            Assert.False(controller.IsPinOpen(24));
            Assert.False(controller.IsPinOpen(13));
        }

        [Fact]
        public void WriteToggleAndReadOnAnOutputKeepIsOnInSync()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            using ExplorerHat hat = new(controller, shouldDispose: false);

            hat.Outputs.Two.Write(PinValue.High);
            Assert.True(hat.Outputs.Two.IsOn);
            Assert.Equal(PinValue.High, driver.GetValue(12));
            Assert.Equal(PinValue.High, hat.Outputs.Two.Read());

            hat.Outputs.Two.Toggle();
            Assert.False(hat.Outputs.Two.IsOn);
            Assert.Equal(PinValue.Low, driver.GetValue(12));

            hat.Outputs.Two.Toggle();
            Assert.True(hat.Outputs.Two.IsOn);
            Assert.Equal(PinValue.High, driver.GetValue(12));

            // Read opens the pin switched off
            Assert.Equal(PinValue.Low, hat.Outputs.Three.Read());
            Assert.True(controller.IsPinOpen(13));
            Assert.False(hat.Outputs.Three.IsOn);
        }

        [Fact]
        public void AnInputCannotBeWrittenOrConfiguredAsOutput()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            Assert.Throws<NotSupportedException>(() => hat.Inputs.One.Write(PinValue.High));
            Assert.Throws<NotSupportedException>(() => hat.Inputs.One.Toggle());
            Assert.Throws<NotSupportedException>(() => hat.Inputs.One.SetPinMode(PinMode.Output));
            Assert.False(controller.IsPinOpen(23));
        }

        [Fact]
        public void AnOutputCannotBeConfiguredAsInput()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            Assert.Throws<NotSupportedException>(() => hat.Outputs.One.SetPinMode(PinMode.Input));
            hat.Outputs.One.SetPinMode(PinMode.Output);
            Assert.False(controller.IsPinOpen(6));
        }

        [Fact]
        public void AnInputDoesNotTakeOverAPinThatIsAlreadyOpen()
        {
            using GpioController controller = new(new FakeGpioDriver());
            ExplorerHat hat = new(controller, shouldDispose: false);

            // The caller opens input 1 with the same controller, for example for another binding
            controller.OpenPin(23, PinMode.Input);

            Assert.Throws<InvalidOperationException>(() => hat.Inputs.One.Read());
            Assert.Throws<InvalidOperationException>(() => hat.Inputs.One.SetPinMode(PinMode.Input));
            hat.Inputs.One.Close();
            hat.Dispose();

            // Neither Close nor Dispose close the pin of the caller
            Assert.True(controller.IsPinOpen(23));
            Assert.Equal(PinValue.Low, controller.Read(23));
        }

        [Fact]
        public void AnOutputDoesNotTakeOverAPinThatIsAlreadyOpen()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            ExplorerHat hat = new(controller, shouldDispose: false);

            // The caller opens output 1 with the same controller and drives it high
            controller.OpenPin(6, PinMode.Output);
            controller.Write(6, PinValue.High);

            Assert.Throws<InvalidOperationException>(() => hat.Outputs.One.On());
            Assert.Throws<InvalidOperationException>(() => hat.Outputs.One.Off());
            Assert.Throws<InvalidOperationException>(() => hat.Outputs.One.Toggle());
            Assert.False(hat.Outputs.One.IsOn);
            hat.Outputs.One.Close();
            hat.Dispose();

            // The pin of the caller is still open and still high
            Assert.True(controller.IsPinOpen(6));
            Assert.Equal(PinValue.High, driver.GetValue(6));
        }

        [Fact]
        public void ClosedInputsAndOutputsOpenTheirPinsAgainOnNextUse()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            using ExplorerHat hat = new(controller, shouldDispose: false);

            hat.Outputs.One.On();
            hat.Inputs.One.Read();
            hat.Outputs.One.Close();
            hat.Inputs.One.Close();
            Assert.False(controller.IsPinOpen(6));
            Assert.False(controller.IsPinOpen(23));
            Assert.Equal(PinValue.Low, driver.GetValue(6));
            Assert.False(hat.Outputs.One.IsOn);

            hat.Outputs.One.On();
            hat.Inputs.One.Read();
            Assert.True(controller.IsPinOpen(6));
            Assert.True(controller.IsPinOpen(23));
        }

        [Fact]
        public void ValueChangedOpensTheInputPinBeforeRegisteringTheCallback()
        {
            using GpioController controller = new(new FakeGpioDriver());
            using ExplorerHat hat = new(controller, shouldDispose: false);

            // FakeGpioDriver does not support callbacks, but the pin must be open by the time the controller is asked
            Assert.Throws<NotSupportedException>(() => hat.Inputs.One.ValueChanged += (sender, args) => { });
            Assert.True(controller.IsPinOpen(23));
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
            Assert.Throws<ObjectDisposedException>(() => hat.Outputs.One.Toggle());
            Assert.Throws<ObjectDisposedException>(() => hat.Outputs.One.Write(PinValue.High));
        }
    }
}
