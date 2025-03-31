// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public class GpioPinTests : IDisposable
    {
        private const int PinNumber = 2;
        private Mock<MockableGpioDriver> _mockedGpioDriver;

        public GpioPinTests()
        {
            _mockedGpioDriver = new Mock<MockableGpioDriver>(MockBehavior.Default);
            _mockedGpioDriver.CallBase = true;
        }

        public void Dispose()
        {
            _mockedGpioDriver.VerifyAll();
        }

        [Fact]
        public void TestOpenPin()
        {
            // Arrange
            _mockedGpioDriver.Setup(x => x.OpenPinEx(PinNumber));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(PinNumber, It.IsAny<PinMode>())).Returns(true);
            _mockedGpioDriver.Setup(x => x.SetPinModeEx(PinNumber, It.IsAny<PinMode>()));
            _mockedGpioDriver.Setup(x => x.GetPinModeEx(PinNumber)).Returns(PinMode.Input);
            var ctrl = new GpioController(_mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            // Assert
            Assert.Equal(PinNumber, pin.PinNumber);
            Assert.Equal(PinMode.Input, pin.GetPinMode());
        }

        /// <summary>
        /// Closes the pin via the controller first
        /// </summary>
        [Fact]
        public void TestClosePin1()
        {
            // Arrange
            _mockedGpioDriver.Setup(x => x.OpenPinEx(PinNumber));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(PinNumber, It.IsAny<PinMode>())).Returns(true);
            _mockedGpioDriver.Setup(x => x.SetPinModeEx(PinNumber, It.IsAny<PinMode>()));
            var ctrl = new GpioController(_mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            ctrl.ClosePin(PinNumber);
            // Closing the pin makes its usage invalid
            Assert.Throws<InvalidOperationException>(() => pin.Read());
            pin.Dispose(); // Shouldn't throw
        }

        /// <summary>
        /// Closes the pin via Dispose first
        /// </summary>
        [Fact]
        public void TestClosePin2()
        {
            // Arrange
            _mockedGpioDriver.Setup(x => x.OpenPinEx(PinNumber));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(PinNumber, It.IsAny<PinMode>())).Returns(true);
            _mockedGpioDriver.Setup(x => x.SetPinModeEx(PinNumber, It.IsAny<PinMode>()));
            var ctrl = new GpioController(_mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            pin.Dispose();
            // Closing the pin makes its usage invalid
            Assert.Throws<InvalidOperationException>(() => ctrl.Read(PinNumber));
            // That is not valid now
            Assert.Throws<InvalidOperationException>(() => ctrl.ClosePin(PinNumber));
        }

        [Fact]
        public void TestToggleReadWrite()
        {
            // Arrange
            PinValue pinValue = PinValue.High;
            _mockedGpioDriver.Setup(x => x.OpenPinEx(PinNumber));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(PinNumber, It.IsAny<PinMode>())).Returns(true);
            _mockedGpioDriver.Setup(x => x.SetPinModeEx(PinNumber, It.IsAny<PinMode>()));
            _mockedGpioDriver.Setup(x => x.ReadEx(PinNumber)).Returns(pinValue);
            var ctrl = new GpioController(_mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            pin.Write(pinValue);
            // Assert
            Assert.Equal(pinValue, pin.Read());
            pin.Toggle();
            // Make sure we setup the drive properly
            pinValue = !pinValue;
            _mockedGpioDriver.Setup(x => x.ReadEx(PinNumber)).Returns(pinValue);
            Assert.Equal(pinValue, pin.Read());
        }
    }
}
