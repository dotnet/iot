// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Board;
using Moq;
using Xunit;

namespace Iot.Device.Gpio.Tests
{
    public sealed class MappingGpioControllerTests : IDisposable
    {
        public Mock<MockableGpioDriver> _driverMock;
        public GpioController _gpioController;

        public MappingGpioControllerTests()
        {
            _driverMock = new Mock<MockableGpioDriver>(MockBehavior.Default);
            _driverMock.CallBase = true;
            _gpioController = new GpioController(_driverMock.Object);
        }

        public void Dispose()
        {
            _driverMock.VerifyAll();
        }

        [Fact]
        public void OpenAndClose()
        {
            // Physical pin 3 maps to logical pin 2.
            var mappedController = RaspberryPiBoard.CreatePhysicalPinMapping(_gpioController);
            _driverMock.Setup(x => x.OpenPinEx(2));
            _driverMock.Setup(x => x.WriteEx(2, PinValue.High));
            _driverMock.Setup(x => x.IsPinModeSupportedEx(2, PinMode.Output)).Returns(true);
            _driverMock.Setup(x => x.GetPinModeEx(2)).Returns(PinMode.Output);
            var pin = mappedController.OpenPin(3, PinMode.Output);
            Assert.NotNull(pin);
            mappedController.IsPinOpen(pin.PinNumber);
            pin.Write(PinValue.High);
            pin.Close();
        }

        [Fact]
        public void OpenAndCloseTwice()
        {
            var mappedController = RaspberryPiBoard.CreatePhysicalPinMapping(_gpioController);
            var pin = mappedController.OpenPin(3);
            Assert.NotNull(pin);
            mappedController.IsPinOpen(pin.PinNumber);
            pin.Write(PinValue.High);
            mappedController.ClosePin(3);
            pin = mappedController.OpenPin(3);
            Assert.True(mappedController.IsPinOpen(3));
            pin.Close();
            Assert.False(mappedController.IsPinOpen(3));
        }

        [Fact]
        public void CannotOpenAnInvalidPin()
        {
            var mappedController = RaspberryPiBoard.CreatePhysicalPinMapping(_gpioController);
            Assert.Throws<InvalidOperationException>(() => mappedController.OpenPin(2)); // Physical pin 2 is a power pin, logical pin 2 would be valid
        }
    }
}
