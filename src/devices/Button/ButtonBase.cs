// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Iot.Device.Button
{
    /// <summary>
    /// Base implementation of Button logic.
    /// Hardware independent. Inherit for specific hardware handling.
    /// </summary>
    public class ButtonBase : IDisposable
    {
        internal static readonly TimeSpan DefaultDoublePressTime = TimeSpan.FromTicks(15000000);
        internal static readonly TimeSpan DefaultHoldingTime = TimeSpan.FromMilliseconds(2000);

        private bool _disposed = false;

        private TimeSpan _doublePressTime;
        private TimeSpan _holdingTime;
        private TimeSpan _debounceTime;
        private TimeSpan _debounceStartTime;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private TimeSpan _lastPress;
        private Timer? _holdingTimer;

        /// <summary>
        /// Delegate for button up event.
        /// </summary>
        public event EventHandler<EventArgs>? ButtonUp;

        /// <summary>
        /// Delegate for button down event.
        /// </summary>
        public event EventHandler<EventArgs>? ButtonDown;

        /// <summary>
        /// The button was pressed. Consistent with the behaviour of a mouse click,
        /// this event is raised when the button is released after being pressed. It is not
        /// raised if the button is held down for a time longer than the configured holding time (and <see cref="IsHoldingEnabled"/> is true).
        /// </summary>
        /// <remarks>Older versions raised the event even if the button was held.</remarks>
        public event EventHandler<EventArgs>? Press;

        /// <summary>
        /// Event for button double pressed event.
        /// </summary>
        public event EventHandler<EventArgs>? DoublePress;

        /// <summary>
        /// Event for button holding event. <see cref="IsHoldingEnabled"/> must be set to true for this event to be raised.
        /// </summary>
        public event EventHandler<ButtonHoldingEventArgs>? Holding;

        /// <summary>
        /// Define if holding event is enabled on this button. If so, the <see cref="Holding"/> event will be raised
        /// when the button is pressed for a time longer than the configured holding time.
        /// Note that the <see cref="Press" /> event will not be raised in case of a holding event.
        /// </summary>
        public bool IsHoldingEnabled { get; set; } = false;

        /// <summary>
        /// Define if double press event is enabled or disabled on the button.
        /// </summary>
        public bool IsDoublePressEnabled { get; set; } = false;

        /// <summary>
        /// Indicates if the button is currently pressed.
        /// </summary>
        public bool IsPressed { get; set; } = false;

        /// <summary>
        /// Initialization of the button with a default double press time and holding time.
        /// </summary>
        public ButtonBase()
            : this(DefaultDoublePressTime, DefaultHoldingTime, default)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="doublePress">Max ticks between button presses to count as doublePress.</param>
        /// <param name="holdingTime">Min ms a button is pressed to count as holding.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public ButtonBase(TimeSpan doublePress, TimeSpan holdingTime, TimeSpan debounceTime)
        {
            if (debounceTime.TotalMilliseconds * 3 > doublePress.TotalMilliseconds)
            {
                throw new ArgumentException($"The parameter {nameof(doublePress)} should be at least three times {nameof(debounceTime)}");
            }

            _doublePressTime = doublePress;
            _holdingTime = holdingTime;
            _debounceTime = debounceTime;
            TimeSource = () => TimeSpan.FromMilliseconds(Environment.TickCount64);
        }

        /// <summary>
        /// This property allows to override the time source used for debouncing.
        /// The default implementation uses GetTickCount64() to get the current time in milliseconds (that's the reason why it is a TimeSpan)
        /// </summary>
        public Func<TimeSpan> TimeSource
        {
            get;
            set;
        }

        /// <summary>
        /// Handler for pressing the button.
        /// </summary>
        protected void HandleButtonPressed()
        {
            if (TimeSource() - _debounceStartTime < _debounceTime)
            {
                return;
            }

            IsPressed = true;

            ButtonDown?.Invoke(this, EventArgs.Empty);

            if (IsHoldingEnabled)
            {
                _holdingTimer = new Timer(StartHoldingHandler, null, _holdingTime, Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary>
        /// Handler for releasing the button.
        /// </summary>
        protected void HandleButtonReleased()
        {
            if (_debounceTime.Ticks > 0 && !IsPressed)
            {
                return;
            }

            _debounceStartTime = TimeSource();
            _holdingTimer?.Dispose();
            _holdingTimer = null;

            IsPressed = false;

            ButtonUp?.Invoke(this, EventArgs.Empty);

            if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
            {
                _holdingState = ButtonHoldingState.Completed;
                Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
            }
            else
            {
                Press?.Invoke(this, EventArgs.Empty);
            }

            if (IsDoublePressEnabled)
            {
                var now = TimeSource();
                if (_lastPress == TimeSpan.Zero)
                {
                    _lastPress = now;
                }
                else
                {
                    if (now - _lastPress <= _doublePressTime)
                    {
                        DoublePress?.Invoke(this, new EventArgs());
                    }

                    _lastPress = now;
                }
            }
        }

        /// <summary>
        /// Handler for holding the button.
        /// </summary>
        private void StartHoldingHandler(object? state)
        {
            _holdingTimer?.Dispose();
            _holdingTimer = null;
            _holdingState = ButtonHoldingState.Started;

            Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Started });
        }

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _holdingTimer?.Dispose();
                _holdingTimer = null;
            }

            _disposed = true;
        }

        /// <summary>
        /// Public dispose method for IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
