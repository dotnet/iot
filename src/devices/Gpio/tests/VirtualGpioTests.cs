// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using System.Device.Spi;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable();
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
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable();
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
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1)).Verifiable();
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

            void Callback(object o, PinValueChangedEventArgs e)
            {
                wasCalled = true;
                callbackAsNo = e.PinNumber;
            }

            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Falling, Callback);

            _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Rising);
            // We expect no callback when we expect a Falling event but a Rising event is triggered
            Assert.False(wasCalled);
            controller.UnregisterCallbackForPinValueChangedEvent(7, Callback);
        }

        [Fact]
        public async Task WaitForEventAsync()
        {
            VirtualGpioController controller = new VirtualGpioController();
            var myPin = _baseController.OpenPin(1);
            controller.Add(7, myPin);
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
            controller.SetPinMode(7, PinMode.Input);
            _mockedGpioDriver.Setup(x => x.WaitForEventEx(1, PinEventTypes.Rising | PinEventTypes.Falling, It.IsAny<CancellationToken>()))
                .Returns(new WaitForEventResult() { EventTypes = PinEventTypes.Falling, TimedOut = false });
            var ret = controller.WaitForEventAsync(7, PinEventTypes.Rising | PinEventTypes.Falling, TimeSpan.FromMinutes(1));
            WaitForEventResult result = await ret;
            Assert.False(result.TimedOut);
            Assert.Equal(PinEventTypes.Falling, result.EventTypes);
        }

        [Fact]
        public void CtorCall()
        {
            var myPin1 = _baseController.OpenPin(99);
            var myPin2 = _baseController.OpenPin(100);
            VirtualGpioController controller = new VirtualGpioController(new GpioPin[] { myPin1, myPin2 });
            Assert.NotNull(controller.GetOpenPin(0));
            Assert.NotNull(controller.GetOpenPin(1));
        }

        [Fact]
        public void CtorCall2()
        {
            var myPin1 = _baseController.OpenPin(99);
            var myPin2 = _baseController.OpenPin(100);
            VirtualGpioController controller = new VirtualGpioController(new Dictionary<int, GpioPin>
            {
                { 5, myPin1 },
                { 9, myPin2 }
            });
            Assert.NotNull(controller.GetOpenPin(5));
            Assert.NotNull(controller.GetOpenPin(9));
            Assert.Throws<InvalidOperationException>(() => controller.GetOpenPin(0));
        }

        [Fact]
        public void QueryComponentInformation()
        {
            var myPin1 = _baseController.OpenPin(99);
            var myPin2 = _baseController.OpenPin(100);
            VirtualGpioController controller = new VirtualGpioController(new Dictionary<int, GpioPin>
            {
                { 5, myPin1 },
                { 9, myPin2 }
            });

            var result = controller.QueryComponentInformation();
            Assert.NotNull(result);
            Assert.NotEmpty(result.SubComponents);
        }
    }
}
