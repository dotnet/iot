﻿// Licensed to the .NET Foundation under one or more agreements.
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
        public void TestOpnPin()
        {
            // Arrange
            var ctrl = new GpioController(PinNumberingScheme.Logical, _mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            // Assert
            Assert.True(pin.PinNumber == PinNumber);
            Assert.True(pin.GetPinMode() == PinMode.Input);
        }

        [Fact]
        public void TestClosePin()
        {
            // Arrange
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
            var ctrl = new GpioController(PinNumberingScheme.Logical, _mockedGpioDriver.Object);
            // Act
            GpioPin pin = ctrl.OpenPin(PinNumber, PinMode.Input);
            pin.Write(PinValue.High);
            // Assert
            Assert.True(pin.Read() == PinValue.High);
            pin.Toggle();
            Assert.True(pin.Read() == PinValue.Low);
        }
    }
}