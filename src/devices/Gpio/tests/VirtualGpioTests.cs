// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
    }
}
