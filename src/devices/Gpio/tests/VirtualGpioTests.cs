// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using System.Device.Spi;
using System.Reflection;
using Moq;
using Xunit;

namespace Iot.Device.Gpio.Tests
{
    public class VirtualGpioTests : IDisposable
    {
        private readonly Mock<MockableGpioDriver> _mockedGpioDriver;
        private readonly GpioController _baseController;

        public VirtualGpioTests()
        {
            _mockedGpioDriver = new Mock<MockableGpioDriver>(MockBehavior.Default);
            _mockedGpioDriver.CallBase = true;
            _baseController = new GpioController(_mockedGpioDriver.Object);
        }

        public void Dispose()
        {
            _baseController.Dispose();
            _mockedGpioDriver.VerifyAll();
        }

        [Fact]
        public void CreateSingleVirtualPin()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            _mockedGpioDriver.Setup(x => x.ReadEx(1)).Returns(PinValue.High);
            controller.Add(7, myPin);
            controller.SetPinMode(7, PinMode.Input);
            Assert.Equal(PinValue.High, controller.Read(7));
        }

        [Fact]
        public void OpenClosePin()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable(Times.Exactly(1));
            controller.ClosePin(7);
        }

        [Fact]
        public void UsePin()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            var newPin = controller.GetOpenPin(7);
            Assert.NotNull(newPin);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            _mockedGpioDriver.Setup(x => x.ReadEx(1)).Returns(PinValue.High);
            newPin.SetPinMode(PinMode.Input);
            Assert.Equal(PinValue.High, newPin.Read());
        }

        [Fact]
        public void ClosePinDirectlyDoesNotThrow()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable(Times.Exactly(1));
            controller.ClosePin(7);
            // Also closing or disposing the original pin shouldn't throw
            myPin.Close();
            myPin.Dispose();
        }

        [Fact]
        public void ClosePinDirectlyDoesNotThrow2()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable(Times.Exactly(1));
            // Close pin first, then close via controller
            myPin.Close();
            myPin.Dispose();
            controller.ClosePin(7);

            Assert.Throws<InvalidOperationException>(() => controller.Read(7));
        }

        [Fact]
        public void AddPinTwiceFails()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            var myOtherPin = _baseController.OpenPin(2);
            controller.Add(7, myPin);
            Assert.False(controller.Add(7, myOtherPin));
            controller.Add(8, myOtherPin);
        }

        [Fact]
        public void CannotOpenPin()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(2, myPin);

            // The exception thrown here is a bit odd, as we're going through the old GpioController
            Assert.Throws<KeyNotFoundException>(() => controller.OpenPin(2));
        }

        [Fact]
        public void Callback1()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            var newPin = controller.GetOpenPin(7);
            Assert.NotNull(newPin);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            newPin.SetPinMode(PinMode.Input);
            bool wasCalled = false;
            int callbackAsNo = 0;
            newPin.ValueChanged += (o, e) =>
            {
                wasCalled = true;
                callbackAsNo = e.PinNumber;
            };

            _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Rising);
            Assert.True(wasCalled);
            Assert.Equal(7, callbackAsNo);
        }

        [Fact]
        public void Callback2()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            controller.SetPinMode(7, PinMode.Input);
            bool wasCalled = false;
            int callbackAsNo = 0;
            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Rising | PinEventTypes.Falling, (o, e) =>
            {
                wasCalled = true;
                callbackAsNo = e.PinNumber;
            });

            _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Rising);
            Assert.True(wasCalled);
            Assert.Equal(7, callbackAsNo);
        }

        [Fact]
        public void Callback3()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            controller.SetPinMode(7, PinMode.Input);
            bool wasCalled = false;
            int callbackAsNo = 0;
            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Falling, (o, e) =>
            {
                wasCalled = true;
                callbackAsNo = e.PinNumber;
            });

            _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Rising);
            // We expect no callback when we expect a Falling event but a Rising event is triggered
            Assert.False(wasCalled);
        }
    }
}
