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

        private long _doublePressTicks;
        private long _holdingMs;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private long _lastPress = DateTime.MinValue.Ticks;
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
        /// Delegate for button pressed event.
        /// </summary>
        public event EventHandler<EventArgs>? Press;

        /// <summary>
        /// Delegate for button double pressed event.
        /// </summary>
        public event EventHandler<EventArgs>? DoublePress;

        /// <summary>
        /// Delegate for button holding event.
        /// </summary>
        public event EventHandler<ButtonHoldingEventArgs>? Holding;

        /// <summary>
        /// Define if holding event is enabled or disabled on the button.
        /// </summary>
        public bool IsHoldingEnabled { get; set; } = false;

        /// <summary>
        /// Define if double press event is enabled or disabled on the button.
        /// </summary>
        public bool IsDoublePressEnabled { get; set; } = false;

        /// <summary>
        /// Define if single press event is enabled or disabled on the button.
        /// </summary>
        public bool IsPressed { get; set; } = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        public ButtonBase()
            : this(TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds))
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="doublePress">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        public ButtonBase(TimeSpan doublePress, TimeSpan holding)
        {
            _doublePressTicks = doublePress.Ticks;
            _holdingMs = (long)holding.TotalMilliseconds;
        }

        /// <summary>
        /// Handler for pressing the button.
        /// </summary>
        protected void HandleButtonPressed()
        {
            IsPressed = true;

            ButtonDown?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled)
            {
                _holdingTimer = new Timer(StartHoldingHandler, null, (int)_holdingMs, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Handler for releasing the button.
        /// </summary>
        protected void HandleButtonReleased()
        {
            _holdingTimer?.Dispose();
            _holdingTimer = null;

            IsPressed = false;

            ButtonUp?.Invoke(this, new EventArgs());
            Press?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
            {
                _holdingState = ButtonHoldingState.Completed;
                Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
            }

            if (IsDoublePressEnabled)
            {
                if (_lastPress == DateTime.MinValue.Ticks)
                {
                    _lastPress = DateTime.UtcNow.Ticks;
                }
                else
                {
                    if (DateTime.UtcNow.Ticks - _lastPress <= _doublePressTicks)
                    {
                        DoublePress?.Invoke(this, new EventArgs());
                    }

                    _lastPress = DateTime.MinValue.Ticks;
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
