// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using System.Threading;

using Moq;
using Tca955x.Tests;
using Xunit;

namespace Iot.Device.Tca955x.Tests
{
    public class Tca9554Tests
    {
        private readonly Tca955xSimulatedDevice _device;
        private readonly Mock<I2cSimulatedDeviceBase> _deviceWithBadAddress;
        private readonly GpioController _controller;
        private readonly Mock<MockableGpioDriver> _driver;

        public Tca9554Tests()
        {
            _device = new Tca955xSimulatedDevice(new I2cConnectionSettings(0, Tca9554.DefaultI2cAddress));
            _deviceWithBadAddress = new Mock<I2cSimulatedDeviceBase>(MockBehavior.Loose, new I2cConnectionSettings(0, Tca9554.DefaultI2cAddress + Tca9554.AddressRange + 1));
            _deviceWithBadAddress.Setup(x => x.ConnectionSettings).CallBase();
            _driver = new Mock<MockableGpioDriver>();
            _driver.CallBase = true;
            _controller = new GpioController(_driver.Object);
        }

        [Fact]
        public void CreateWithInterrupt()
        {
            var testee = new Tca9554(_device, 10, _controller);
        }

        [Fact]
        public void CreateWithBadAddress()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Tca9554(_deviceWithBadAddress.Object, -1));
        }

        [Fact]
        public void CreateWithoutInterrupt()
        {
            var testee = new Tca9554(_device, -1);
        }

        [Fact]
        public void TestRead()
        {
            var testee = new Tca9554(_device, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(8, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            var value = pin0.Read();
            Assert.Equal(PinValue.High, value);
            pin0.Dispose();
            Assert.False(tcaController.IsPinOpen(0));
        }

        [Fact]
        public void InterruptCallbackIsInvokedOnPinChange()
        {
            // Arrange
            var interruptPin = 10;
            var testee = new Tca9554(_device, interruptPin, _controller);
            var tcaController = new GpioController(testee);
            tcaController.OpenPin(1, PinMode.Input);
            bool callbackInvoked = false;
            PinValueChangedEventArgs? receivedArgs = null;
            ManualResetEventSlim mre = new(false);

            void Callback(object sender, PinValueChangedEventArgs args)
            {
                callbackInvoked = true;
                receivedArgs = args;
                mre.Set();
            }

            _device.SetPinState(1, PinValue.High);

            // Register callback for rising edge
            tcaController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Falling, Callback);

            // Change the device setup to simulate pin1 as low.
            _device.SetPinState(1, PinValue.Low);
            // Act
            // Simulate the hardware int pin change using the _controller mock
            _driver.Object.FireEventHandler(interruptPin, PinEventTypes.Rising);
            mre.Wait(2000); // Wait for the callback to be invoked

            // Assert
            Assert.True(callbackInvoked);
            Assert.NotNull(receivedArgs);
            Assert.Equal(1, receivedArgs.PinNumber);
            Assert.Equal(PinEventTypes.Falling, receivedArgs.ChangeType);
        }

        [Fact]
        public void TestReadOfIllegalPinThrows()
        {
            var testee = new Tca9554(_device, -1);
            var tcaController = new GpioController(testee);
            Assert.Equal(8, tcaController.PinCount);
            GpioPin pin0 = tcaController.OpenPin(0);
            Assert.NotNull(pin0);
            Assert.True(tcaController.IsPinOpen(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => tcaController.Read(new Span<PinValuePair>(new PinValuePair[] { new(9, PinValue.Low) })));
        }

        [Fact]
        public void CanNotConstructIfInterruptConfiguredIncorrectly()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var testee = new Tca9554(_device, -1, _controller);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var testee = new Tca9554(_device, 2);
            });
        }

    }
}
