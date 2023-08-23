// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task If_Button_Is_Held_Holding_Event_Fires()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            // we set short times to avoid wasting when executing the tests
            var debounceTime = TimeSpan.FromMilliseconds(200);
            var holdingTime = TimeSpan.FromMilliseconds(400);
            TestButton button = new TestButton(debounceTime, holdingTime);
            button.IsHoldingEnabled = true;
            TaskCompletionSource<DateTime> tcs = new TaskCompletionSource<DateTime>();

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
                if (e.HoldingState == ButtonHoldingState.Completed)
                {
                    tcs.SetResult(DateTime.Now);
                }
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            DateTime now = DateTime.Now;
            button.PressButton();

            Thread.Sleep((int)holdingTime.TotalMilliseconds + 100);

            button.ReleaseButton();

            // this is only needed to avoid to wait indefinitely in case the code gets broken and the test fail
            var firstTask = await Task.WhenAny(tcs.Task, Task.Delay(2 * (int)holdingTime.TotalMilliseconds));
            Assert.True(tcs.Task == firstTask, "holding timeout");

            // holdingTime is the DateTime retrieved in the holding timer handler
            var effectiveHoldingTime = tcs.Task.Result;

            Assert.True(effectiveHoldingTime - now >= holdingTime, "holding");
            Assert.True(holding, "holding");
            Assert.True(pressed, "pressed");
            Assert.False(doublePressed, "doublePressed");
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

            var holdingTime = TimeSpan.FromMilliseconds(2000);
            TestButton button = new TestButton(TimeSpan.FromMilliseconds(1000), holdingTime);

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

        /// <summary>
        /// From issue #1877
        /// The problem arises when the button is held down for longer then the debounce timeout.
        /// Then, as it is released there will be a "pressed" event caused by the bounces
        /// happening during release, and the desired "released" event is fired,
        /// due to the debouncing getting started by "pressed"
        /// </summary>
        [Fact]
        public void If_Button_Is_Held_Down_Longer_Than_Debouncing()
        {
            bool holding = false;
            bool doublePressed = false;
            int buttonDownCounter = 0;
            int buttonUpCounter = 0;
            int pressedCounter = 0;

            // holding is 2 secs, debounce is 1 sec
            var holdingTime = TimeSpan.FromMilliseconds(2000);
            TestButton button = new TestButton(TimeSpan.FromMilliseconds(1000), holdingTime);
            button.IsHoldingEnabled = true;

            button.Press += (sender, e) =>
            {
                pressedCounter++;
            };

            button.ButtonDown += (sender, e) =>
            {
                buttonDownCounter++;
            };

            button.ButtonUp += (sender, e) =>
            {
                buttonUpCounter++;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            // pushing the button. This will trigger the buttonDown event
            button.PressButton();
            Thread.Sleep(2200);
            // releasing the button. This will trigger the pressed and buttonUp event
            button.ReleaseButton();

            // now simulating hw bounces which should not be detected
            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();
            button.PressButton();
            button.ReleaseButton();

            Assert.True(buttonDownCounter == 1, "ButtonDown counter is wrong");
            Assert.True(buttonUpCounter == 1, "ButtonUp counter is wrong");
            Assert.True(pressedCounter == 1, "pressedCounter counter is wrong");
            Assert.True(holding, "holding");
            Assert.False(doublePressed, "doublePressed");
        }
    }
}
