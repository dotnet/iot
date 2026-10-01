// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

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

            using TestButton button = new TestButton();

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
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

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
            bool released = false;

            var debounceTime = TimeSpan.FromMilliseconds(200);
            var holdingTime = TimeSpan.FromMilliseconds(400);
            using TestButton button = new TestButton(debounceTime, holdingTime);
            button.IsHoldingEnabled = true;

            button.Press += (sender, e) =>
            {
                pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                holding = true;
                if (e.HoldingState == ButtonHoldingState.Completed)
                {
                    released = true;
                }
            };

            button.DoublePress += (sender, e) =>
            {
                doublePressed = true;
            };

            button.PressButton();

            button.TimeProvider.Advance(holdingTime - TimeSpan.FromMilliseconds(1));
            Assert.False(holding);

            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(1));
            Assert.True(holding);
            Assert.False(released);

            button.ReleaseButton();

            Assert.True(released, "released");
            Assert.False(pressed, "pressed");
            Assert.False(doublePressed, "doublePressed");
        }

        [Fact]
        public void If_Button_Is_Held_And_Holding_Is_Disabled_Holding_Event_Does_Not_Fire()
        {
            bool pressed = false;
            bool holding = false;
            bool doublePressed = false;

            using TestButton button = new TestButton();
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
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(2100));

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

            using TestButton button = new TestButton();
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
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(200));

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

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

            using TestButton button = new TestButton();

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
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

            button.ReleaseButton();

            // Wait longer than default double press threshold milliseconds, for the press to be recognized as two separate presses.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(3000));

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

            button.ReleaseButton();

            Assert.True(pressed);
            Assert.False(holding);
            Assert.False(doublePressed);
        }

        [Fact]
        public void If_Button_Is_Double_Pressed_And_DoublePress_Is_Disabled_DoublePress_Event_Does_Not_Fire()
        {
            bool pressed = false;

            using TestButton button = new TestButton();
            button.IsDoublePressEnabled = false;

            button.DoublePress += (sender, e) =>
            {
                pressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(200));

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));

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
            using TestButton button = new TestButton(TimeSpan.FromMilliseconds(1000), holdingTime);

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
            using TestButton button = new TestButton(TimeSpan.FromMilliseconds(1000), holdingTime);
            button.IsHoldingEnabled = true;

            button.Press += (sender, e) =>
            {
                // This is not triggered when holding
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
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(2200));
            // releasing the button. This will trigger the holding and buttonUp event
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
            Assert.Equal(0, pressedCounter);
            Assert.True(holding, "holding");
            Assert.False(doublePressed, "doublePressed");
        }

        [Fact]
        public void If_TimeProvider_Is_Null_Constructor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ButtonBase(null!));
            Assert.Throws<ArgumentNullException>(() => new ButtonBase(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2), TimeSpan.Zero, null!));
        }

        [Fact]
        public void If_Debounce_Time_Has_Elapsed_Next_Press_Is_Accepted()
        {
            var debounceTime = TimeSpan.FromSeconds(1);
            using TestButton button = new TestButton(debounceTime, TimeSpan.FromSeconds(2));
            int pressedCounter = 0;
            button.Press += (sender, e) => pressedCounter++;

            button.PressButton();
            button.ReleaseButton();
            Assert.Equal(1, pressedCounter);

            button.TimeProvider.Advance(debounceTime - TimeSpan.FromTicks(1));
            button.PressButton();
            button.ReleaseButton();
            Assert.Equal(1, pressedCounter);

            button.TimeProvider.Advance(TimeSpan.FromTicks(1));
            button.PressButton();
            button.ReleaseButton();
            Assert.Equal(2, pressedCounter);
        }

        [Theory]
        [InlineData(-1, true)]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void DoublePress_Respects_Time_Threshold(long extraTicks, bool expectedDoublePress)
        {
            using TestButton button = new TestButton();
            button.IsDoublePressEnabled = true;
            bool doublePressed = false;
            button.DoublePress += (sender, e) => doublePressed = true;

            button.PressButton();
            button.ReleaseButton();
            button.TimeProvider.Advance(TimeSpan.FromMilliseconds(1500) + TimeSpan.FromTicks(extraTicks));
            button.PressButton();
            button.ReleaseButton();

            Assert.Equal(expectedDoublePress, doublePressed);
        }

        [Fact]
        public void DoublePress_Is_Raised_For_Nonoverlapping_Pairs()
        {
            using TestButton button = new TestButton();
            button.IsDoublePressEnabled = true;
            int doublePressedCounter = 0;
            button.DoublePress += (sender, e) => doublePressedCounter++;

            for (int i = 1; i <= 4; i++)
            {
                button.PressButton();
                button.ReleaseButton();
                Assert.Equal(i / 2, doublePressedCounter);
                button.TimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            }
        }

        [Fact]
        public void If_Button_Is_Released_Before_Holding_Time_Timer_Is_Cancelled()
        {
            var holdingTime = TimeSpan.FromSeconds(2);
            using TestButton button = new TestButton(TimeSpan.Zero, holdingTime);
            button.IsHoldingEnabled = true;
            bool holding = false;
            bool pressed = false;
            button.Holding += (sender, e) => holding = true;
            button.Press += (sender, e) => pressed = true;

            button.PressButton();
            button.TimeProvider.Advance(holdingTime - TimeSpan.FromMilliseconds(1));
            button.ReleaseButton();
            button.TimeProvider.Advance(holdingTime);

            Assert.True(pressed);
            Assert.False(holding);
        }

        [Fact]
        public void If_Button_Is_Disposed_Holding_Timer_Is_Cancelled()
        {
            using TestButton button = new TestButton();
            button.IsHoldingEnabled = true;
            bool holding = false;
            button.Holding += (sender, e) => holding = true;

            button.PressButton();
            button.Dispose();
            button.TimeProvider.Advance(TimeSpan.FromSeconds(3));

            Assert.False(holding);
        }
    }
}
