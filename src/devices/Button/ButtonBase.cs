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
        internal const long DefaultDoublePressTicks = 15000000;
        internal const long DefaultHoldingMilliseconds = 2000;

        private bool _disposed = false;

        private readonly TimeProvider _timeProvider;
        private TimeSpan _doublePressTime;
        private TimeSpan _holdingTime;
        private TimeSpan _debounceTime;
        private long? _debounceStartTimestamp;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private long? _lastPress;
        private ITimer? _holdingTimer;

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
        /// Initialization of the button.
        /// </summary>
        public ButtonBase()
            : this(TimeProvider.System)
        {
        }

        /// <summary>
        /// Initialization of the button with default timings and a time provider.
        /// </summary>
        /// <param name="timeProvider">The provider used to measure elapsed time and create holding timers.</param>
        public ButtonBase(TimeProvider timeProvider)
            : this(TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds), default, timeProvider)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="doublePress">Max ticks between button presses to count as doublePress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public ButtonBase(TimeSpan doublePress, TimeSpan holding, TimeSpan debounceTime)
            : this(doublePress, holding, debounceTime, TimeProvider.System)
        {
        }

        /// <summary>
        /// Initialization of the button with a time provider.
        /// </summary>
        /// <param name="doublePress">Maximum time between button presses to count as a double press.</param>
        /// <param name="holding">Minimum time a button is pressed to count as holding.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero.</param>
        /// <param name="timeProvider">The provider used to measure elapsed time and create holding timers.</param>
        public ButtonBase(TimeSpan doublePress, TimeSpan holding, TimeSpan debounceTime, TimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

            if (debounceTime.TotalMilliseconds * 3 > doublePress.TotalMilliseconds)
            {
                throw new ArgumentException($"The parameter {nameof(doublePress)} should be at least three times {nameof(debounceTime)}");
            }

            _doublePressTime = doublePress;
            _holdingTime = holding;
            _debounceTime = debounceTime;
        }

        /// <summary>
        /// Handler for pressing the button.
        /// </summary>
        protected void HandleButtonPressed()
        {
            if (_debounceStartTimestamp.HasValue && _timeProvider.GetElapsedTime(_debounceStartTimestamp.Value) < _debounceTime)
            {
                return;
            }

            IsPressed = true;

            ButtonDown?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled)
            {
                _holdingTimer = _timeProvider.CreateTimer(StartHoldingHandler, null, _holdingTime, Timeout.InfiniteTimeSpan);
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

            _debounceStartTimestamp = _timeProvider.GetTimestamp();
            _holdingTimer?.Dispose();
            _holdingTimer = null;

            IsPressed = false;

            ButtonUp?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
            {
                _holdingState = ButtonHoldingState.Completed;
                Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
            }
            else
            {
                Press?.Invoke(this, new EventArgs());
            }

            if (IsDoublePressEnabled)
            {
                if (!_lastPress.HasValue)
                {
                    _lastPress = _timeProvider.GetTimestamp();
                }
                else
                {
                    if (_timeProvider.GetElapsedTime(_lastPress.Value) <= _doublePressTime)
                    {
                        DoublePress?.Invoke(this, new EventArgs());
                    }

                    _lastPress = null;
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
