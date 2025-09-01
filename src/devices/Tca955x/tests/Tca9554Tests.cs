// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using Moq;
using Xunit;

namespace Iot.Device.Tca955x.Tests
{
    public class Tca9554Tests
    {
        private readonly Mock<MockableI2cDevice> _device;
        private readonly Mock<MockableI2cDevice> _deviceWithBadAddress;
        private readonly GpioController _controller;
        private readonly Mock<MockableGpioDriver> _driver;

        public Tca9554Tests()
        {
            _device = new Mock<MockableI2cDevice>(MockBehavior.Loose);
            _deviceWithBadAddress = new Mock<MockableI2cDevice>(MockBehavior.Loose);
            _device.CallBase = true;
            _driver = new Mock<MockableGpioDriver>();
            _driver.CallBase = true;
            _controller = new GpioController(_driver.Object);
            _device.Setup(x => x.ConnectionSettings).Returns(new I2cConnectionSettings(0, Tca9554.DefaultI2cAddress));
            _deviceWithBadAddress.Setup(x => x.ConnectionSettings).Returns(new I2cConnectionSettings(0, Tca9554.DefaultI2cAddress + Tca9554.AddressRange + 1));
        }

        [Fact]
        public void CreateWithInterrupt()
        {
            var testee = new Tca9554(_device.Object, 10, _controller);
            _driver.VerifyAll();
            _device.VerifyAll();

        }

        [Fact]
        public void CreateWithBadAddress()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Tca9554(_deviceWithBadAddress.Object, -1));
            _driver.VerifyAll();
            _deviceWithBadAddress.VerifyAll();

        }

        [Fact]
        public void CreateWithoutInterrupt()
        {
            var testee = new Tca9554(_device.Object, -1);
            _driver.VerifyAll();
            _device.VerifyAll();

        }

        [Fact]
        public void TestRead()
        {
            _device.Setup(x => x.Write(new byte[1]
            {
                0
            }));
            _device.Setup(x => x.Read(It.IsAny<byte[]>())).Callback((byte[] b) =>
            {
                b[0] = 1;
            });

            var testee = new Tca9554(_device.Object, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(8, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            var value = pin0.Read();
            Assert.Equal(PinValue.High, value);
            pin0.Dispose();
            Assert.False(tcaController.IsPinOpen(0));
            _driver.VerifyAll();
            _device.VerifyAll();

        }

        [Fact]
        public void InterruptCallbackIsInvokedOnPinChange()
        {
            // Arrange
            var interruptPin = 10;
            var testee = new Tca9554(_device.Object, interruptPin, _controller);
            var tcaController = new GpioController(testee);
            tcaController.OpenPin(1, PinMode.Input);
            bool callbackInvoked = false;
            PinValueChangedEventArgs? receivedArgs = null;

            void Callback(object sender, PinValueChangedEventArgs args)
            {
                callbackInvoked = true;
                receivedArgs = args;
            }

            // Change the device setup to simulate pin1 as high
            _device.Setup(x => x.Read(It.IsAny<byte[]>())).Callback((byte[] b) =>
            {
                b[0] = 0x02;
            });

            // Register callback for rising edge
            tcaController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Falling, Callback);

            // Change the device setup to simulate pin1 as low.
            _device.Setup(x => x.Read(It.IsAny<byte[]>())).Callback((byte[] b) =>
            {
                b[0] = 0x00;
            });

            // Simulate the hardware int pin pin change using the _controller mock
            _driver.Object.FireEventHandler(interruptPin, PinEventTypes.Rising);

            // Assert
            Assert.True(callbackInvoked);
            Assert.NotNull(receivedArgs);
            Assert.Equal(1, receivedArgs.PinNumber);
            Assert.Equal(PinEventTypes.Falling, receivedArgs.ChangeType);
        }

        [Fact]
        public void RegisterCallbackThrowsIfNoInterruptConfigured()
        {
            var testee = new Tca9554(_device.Object, -1, _controller);
            var tcaController = new GpioController(testee);
            tcaController.OpenPin(1, PinMode.Input);
            void Callback(object sender, PinValueChangedEventArgs args)
            {
                // No-op
            }

            Assert.Throws<InvalidOperationException>(() =>
            {
                tcaController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, Callback);
            });
        }

    }
}
