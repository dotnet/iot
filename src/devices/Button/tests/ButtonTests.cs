// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

using Xunit;

namespace Iot.Device.Button.Tests
{
    public class ButtonTests
    {
        [Fact]
        public void If_Button_Is_Once_Pressed_Press_Event_Fires()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            TestButton button = new TestButton();

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.False(holding);
            Assert.False(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Held_Holding_Event_Fires()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            TestButton button = new TestButton();
            button.IsHoldingEnabled = true;

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            // Wait longer than default holding threshold milliseconds, for the click to be recognized as a holding event.
            Thread.Sleep(2100);

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.True(holding);
            Assert.False(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Held_And_Holding_Is_Disabled_Holding_Event_Does_Not_Fire()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            TestButton button = new TestButton();
            button.IsHoldingEnabled = false;

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            // Wait longer than default holding threshold milliseconds, for the press to be recognized as a holding event.
            Thread.Sleep(2100);

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.False(holding);
            Assert.False(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Double_Pressed_DoublePress_Event_Fires()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            TestButton button = new TestButton();
            button.IsDoublePressEnabled = true;

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.False(holding);
            Assert.True(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Pressed_Twice_DoublePress_Event_Does_Not_Fire()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            TestButton button = new TestButton();

            button.IsDoublePressEnabled = true;

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait longer than default double press threshold milliseconds, for the press to be recognized as two separate presses.
            Thread.Sleep(3000);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.False(holding);
            Assert.False(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Double_Pressed_And_DoublePress_Is_Disabled_DoublePress_Event_Does_Not_Fire()
        {
            bool pressed = false;

            TestButton button = new TestButton();
            button.IsDoublePressEnabled = false;

            button.DoublePress += (sender, e) =>
            {
                pressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.False(pressed);
        }

        [Fact]
        public void If_Button_Is_Pressed_Too_Fast_Debouncing_Removes_Events()
        {
            bool holding = false;
            bool doublePressed = false;
            int pressedCounter = 0;

            TestButton button = new TestButton(TimeSpan.FromMilliseconds(1000));

            button.Press += (sender, e) =>
            {
                pressedCounter++;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();

            Assert.Equal(1, pressedCounter);
            Assert.False(holding);
            Assert.False(doublePressed);
        }

    }
}
