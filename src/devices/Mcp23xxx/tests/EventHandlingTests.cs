// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Mcp23xxx;
using Iot.Device.Mcp23xxx.Tests;
using Xunit;
using Moq;

namespace Iot.Device.Mcp23xxx.Tests
{
    public sealed class EventHandlingTests : Mcp23xxxTest, IDisposable
    {
        private I2cDeviceMock _mockI2c;
        private GpioDriverMock _driverMock;
        private Iot.Device.Mcp23xxx.Mcp23xxx _device;
        private GpioController _gpioController;
        private int _callbackNo;

        public EventHandlingTests()
        {
            _callbackNo = 0;
            _mockI2c = new I2cDeviceMock(2, null);
            _driverMock = new GpioDriverMock();
            _gpioController = new GpioController(_driverMock);
            _device = new Mcp23017(_mockI2c, -1, 11, 22, _gpioController, false);
        }

        [Fact]
        public void EnableDisableEvents()
        {
            _device.EnableInterruptOnChange(0, PinEventTypes.Falling | PinEventTypes.Rising);
            _device.DisableInterruptOnChange(0);
        }

        [Fact]
        public void AddEventHandlerPortA()
        {
            GpioController theDeviceController = new GpioController(_device);
            theDeviceController.OpenPin(1, PinMode.Input);
            theDeviceController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, Callback);

            _mockI2c.DeviceMock.Registers[14] = 2; // Port A INTF register (pin 1 triggered the event)
            _mockI2c.DeviceMock.Registers[0x12] = 2; // Port A GPIO register (pin 1 is high now)
            // This should simulate the interrupt being triggered on the master controller, not the mcp!
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Falling, 11));
            Assert.True(_callbackNo == 1);
            // Nothing registered for an event on interrupt B, so this shouldn't do anything
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Falling, 22));
            Assert.True(_callbackNo == 1);
        }

        [Fact]
        public void AddEventHandlerPortB()
        {
            GpioController theDeviceController = new GpioController(_device);
            theDeviceController.OpenPin(10, PinMode.Input);
            theDeviceController.RegisterCallbackForPinValueChangedEvent(10, PinEventTypes.Rising, Callback);

            _mockI2c.DeviceMock.Registers[0x0F] = 4; // Port B INTF register (pin 1 triggered the event)
            _mockI2c.DeviceMock.Registers[0x13] = 4; // Port B GPIO register (pin 1 is high now)
            // This should simulate the interrupt being triggered on the master controller, not the mcp!
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Falling, 22));
            Assert.True(_callbackNo == 1);
            // Nothing registered for an event on interrupt B, so this shouldn't do anything
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Falling, 11));
            Assert.True(_callbackNo == 1);
        }

        [Fact]
        public void AddMultipleEventHandlers()
        {
            _callbackNo = 0;
            GpioController theDeviceController = new GpioController(_device);
            theDeviceController.OpenPin(0, PinMode.Input);
            theDeviceController.RegisterCallbackForPinValueChangedEvent(0, PinEventTypes.Rising | PinEventTypes.Falling, Callback2);

            theDeviceController.OpenPin(1, PinMode.Input);
            theDeviceController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising | PinEventTypes.Falling, Callback2);

            _mockI2c.DeviceMock.Registers[0x0E] = 1; // Port A INTF register (pin 0 triggered the event)
            _mockI2c.DeviceMock.Registers[0x12] = 3; // Port A GPIO register (pin 0 and 1 are high)
            // This should simulate the interrupt being triggered on the master controller, not the mcp!
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Rising, 11));
            Assert.Equal(3, _callbackNo);
        }

        [Fact]
        public void AddRemoveEventHandler()
        {
            GpioController theDeviceController = new GpioController(_device);
            theDeviceController.OpenPin(1, PinMode.Input);
            theDeviceController.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, Callback);
            theDeviceController.UnregisterCallbackForPinValueChangedEvent(1, Callback);

            // Now trigger an event, shouldn't do anything
            _mockI2c.DeviceMock.Registers[14] = 2; // Port A INTF register (pin 1 triggered the event)
            _mockI2c.DeviceMock.Registers[0x12] = 2; // Port A GPIO register (pin 1 is high now)
            // This should simulate the interrupt being triggered on the master controller, not the mcp!
            _driverMock.FireEvent(new PinValueChangedEventArgs(PinEventTypes.Falling, 11));
            Assert.True(_callbackNo == 0);
            theDeviceController.ClosePin(1);
        }

        private void Callback(object sender, PinValueChangedEventArgs e)
        {
            Assert.Equal(PinEventTypes.Rising, e.ChangeType);
            Assert.True(e.PinNumber == 1 || e.PinNumber == 10);
            _callbackNo++;
        }

        private void Callback2(object sender, PinValueChangedEventArgs e)
        {
            if (e.PinNumber == 0)
            {
                _callbackNo |= 1;
            }
            else if (e.PinNumber == 1)
            {
                _callbackNo |= 2;
            }
        }

        public void Dispose()
        {
            _gpioController.Dispose();
            _device.Dispose();
        }
    }
}
