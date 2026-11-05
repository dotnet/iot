// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;

using Moq;
using Xunit;

namespace Iot.Device.Button.Tests
{
    public class GpioButtonTests
    {
        private const int ButtonPin = 12;

        private readonly Mock<MockableGpioDriver> _driver;

        public GpioButtonTests()
        {
            _driver = new Mock<MockableGpioDriver>();
            _driver.CallBase = true;
            _driver.Setup(x => x.IsPinModeSupportedEx(ButtonPin, It.IsAny<PinMode>())).Returns(true);
        }

        [Fact]
        public void If_Button_Is_Created_And_Disposed_Pin_Is_Opened_And_Closed()
        {
            GpioButton button = CreateButton(PinValue.High);

            _driver.Verify(x => x.OpenPinEx(ButtonPin), Times.Once);
            _driver.Verify(x => x.SetPinModeEx(ButtonPin, PinMode.InputPullUp), Times.Once);
            _driver.Verify(x => x.AddCallbackForPinValueChangedEventEx(ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, It.IsAny<PinChangeEventHandler>()), Times.Once);

            button.Dispose();

            _driver.Verify(x => x.RemoveCallbackForPinValueChangedEventEx(ButtonPin, It.IsAny<PinChangeEventHandler>()), Times.Once);
            _driver.Verify(x => x.ClosePinEx(ButtonPin), Times.Once);
        }

        [Theory]
        [InlineData(true, 0, true)] // pull-up: pressed pulls the pin Low
        [InlineData(true, 1, false)]
        [InlineData(false, 1, true)] // pull-down: pressed pulls the pin High
        [InlineData(false, 0, false)]
        public void If_Button_Is_Created_IsPressed_Reflects_Pin_Level(bool isPullUp, int levelAtStartup, bool expectedIsPressed)
        {
            using GpioButton button = CreateButton(levelAtStartup, isPullUp);

            Assert.Equal(expectedIsPressed, button.IsPressed);
        }

        [Fact]
        public void If_Button_Has_External_PullUp_And_Pin_Is_Low_At_Startup_Button_Is_Pressed()
        {
            using GpioButton button = CreateButton(PinValue.Low, isPullUp: true, hasExternalResistor: true);

            _driver.Verify(x => x.SetPinModeEx(ButtonPin, PinMode.Input), Times.Once);
            Assert.True(button.IsPressed);
        }

        [Fact]
        public void If_Button_Is_Held_At_Startup_With_Debouncing_Release_Raises_ButtonUp_And_Press()
        {
            bool buttonUp = false;
            bool pressed = false;

            using GpioButton button = CreateButton(PinValue.Low, debounceTime: TimeSpan.FromMilliseconds(100));
            button.ButtonUp += (sender, e) => buttonUp = true;
            button.Press += (sender, e) => pressed = true;

            _driver.Object.FireEventHandler(ButtonPin, PinEventTypes.Rising);

            Assert.True(buttonUp);
            Assert.True(pressed);
            Assert.False(button.IsPressed);
        }

        private GpioButton CreateButton(PinValue levelAtStartup, bool isPullUp = true, bool hasExternalResistor = false, TimeSpan debounceTime = default)
        {
            _driver.Setup(x => x.ReadEx(ButtonPin)).Returns(levelAtStartup);
            return new GpioButton(ButtonPin, isPullUp, hasExternalResistor, new GpioController(_driver.Object), shouldDispose: true, debounceTime);
        }
    }
}
