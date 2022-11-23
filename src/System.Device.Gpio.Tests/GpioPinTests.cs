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
            var ctrl = new GpioController(PinNumberingScheme.Logical, _mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            // Assert
            Assert.Equal(pin.PinNumber, PinNumber);
            Assert.Equal(PinMode.Input, pin.GetPinMode());
        }

        [Fact]
        public void TestClosePin()
        {
            // Arrange
            _mockedGpioDriver.Setup(x => x.OpenPinEx(PinNumber));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(PinNumber, It.IsAny<PinMode>())).Returns(true);
            _mockedGpioDriver.Setup(x => x.SetPinModeEx(PinNumber, It.IsAny<PinMode>()));
            var ctrl = new GpioController(PinNumberingScheme.Logical, _mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            ctrl.ClosePin(PinNumber);
            // Assert
            // This should work even if the pin is closed in the controller as the driver has no idea
            // Of the controller behavior.
            var ret = pin.Read();
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
            var ctrl = new GpioController(PinNumberingScheme.Logical, _mockedGpioDriver.Object);
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
