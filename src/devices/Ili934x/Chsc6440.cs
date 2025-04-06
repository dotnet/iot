// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Ili934x
{
    /// <summary>
    /// Binding for Chipsemi CHSC6540 capacitive touch screen controller
    /// Used for instance on the M5Tough in conjunction with an ILI9342 display controller.
    /// Note: The M5Core2, while being very similar to the M5Tough otherwise, has a FT6336U instead.
    /// The two chips appear to be similar, but the documentation is poor.
    /// </summary>
    public class Chsc6440 : IDisposable
    {
        /// <summary>
        /// The default I2C address of this chip
        /// </summary>
        public const int DefaultI2cAddress = 0x2E;

        private readonly int _interruptPin;
        private readonly bool _shouldDispose;
        private readonly bool _flipScreen;

        private GpioController? _gpioController;
        private I2cDevice _i2c;

        private bool _wasRead;

        private TimeSpan _interval;

        private Point[] _lastPoints;
        private Point _initialTouchPoint;
        private Point[] _points;

        private int _activeTouches;
        private int _lastActiveTouches;
        private bool _dragging;

        private bool _isPressed;

        private Thread? _updateThread;
        private bool _updateThreadActive;
        private object _lock;
        private AutoResetEvent _updateEvent;

        /// <summary>
        /// This event is fired when the user "clicks" a position
        /// Call <see cref="EnableEvents"/> to use event handling
        /// </summary>
        public event Action<object, Point>? Touched;

        /// <summary>
        /// This event is fired repeatedly when the user drags over the screen
        /// Call <see cref="EnableEvents"/> to use event handling.
        /// </summary>
        public event Action<object, DragEventArgs>? Dragging;

        /// <summary>
        /// The event that is fired when the user zooms (using two fingers)
        /// Call <see cref="EnableEvents"/> to use event handling.
        /// The second argument is the list of touch points (always 2 when this function is called), the third and fourth
        /// argument are the old and the new distance between the points. So if the value decreases, zooming out is intended.
        /// The values are always &gt; 0
        /// </summary>
        public event Action<object, Point[], int, int>? Zooming;

        /// <summary>
        /// Create a controller from the given I2C device
        /// </summary>
        /// <param name="device">An I2C device</param>
        /// <param name="screenSize">Size of the screen. Used to filter out invalid readings</param>"/>
        /// <param name="interruptPin">The interrupt pin to use, -1 to disable</param>
        /// <param name="gpioController">The gpio controller the interrupt pin is attached to</param>
        /// <param name="shouldDispose">True to dispose the gpio controller on close</param>
        public Chsc6440(I2cDevice device, Size screenSize, int interruptPin = -1, GpioController? gpioController = null, bool shouldDispose = true)
            : this(device, screenSize, false, interruptPin, gpioController, shouldDispose)
        {
        }

        /// <summary>
        /// Create a controller from the given I2C device
        /// </summary>
        /// <param name="device">An I2C device</param>
        /// <param name="screenSize">Size of the screen. Used to filter out invalid readings</param>"/>
        /// <param name="flipScreen">Rotate screen upside down (replace x with width-x and y with height-y)</param>
        /// <param name="interruptPin">The interrupt pin to use, -1 to disable</param>
        /// <param name="gpioController">The gpio controller the interrupt pin is attached to</param>
        /// <param name="shouldDispose">True to dispose the gpio controller on close</param>
        public Chsc6440(I2cDevice device, Size screenSize, bool flipScreen, int interruptPin = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            ScreenSize = screenSize;
            _i2c = device;
            _flipScreen = flipScreen;
            _interruptPin = interruptPin;
            _gpioController = gpioController;
            _shouldDispose = shouldDispose;
            _wasRead = false;
            _points = new Point[2];
            _lastPoints = new Point[2];
            _initialTouchPoint = Point.Empty;
            _activeTouches = 0;
            _lastActiveTouches = 0;
            _dragging = false;
            _interval = TimeSpan.FromMilliseconds(20);
            _updateThread = null;
            _lock = new object();
            _updateEvent = new AutoResetEvent(false);
            TouchSize = new Size(5, 5);

            Span<byte> initData = stackalloc byte[2]
            {
                0x5A, 0x5A
            };
            _i2c.Write(initData);
            if (_interruptPin >= 0)
            {
                if (_gpioController == null)
                {
                    throw new ArgumentNullException(nameof(gpioController));
                }

                _gpioController.OpenPin(_interruptPin, PinMode.InputPullUp);
                _gpioController.RegisterCallbackForPinValueChangedEvent(_interruptPin, PinEventTypes.Rising | PinEventTypes.Falling, OnInterrupt);
            }
        }

        /// <summary>
        /// Size of the screen
        /// </summary>
        public Size ScreenSize { get; }

        /// <summary>
        /// Sets the background thread update interval. Low values can impact performance, but increase the responsiveness.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get
            {
                return _interval;
            }
            set
            {
                _interval = value;
            }
        }

        /// <summary>
        /// The size of the rectangle that is considered a "touch". When the position changes more than this, it is considered a drag.
        /// </summary>
        public Size TouchSize
        {
            get;
            set;
        }

        private Point FlipPointIfNeeded(Point pt)
        {
            if (_flipScreen)
            {
                return new Point(ScreenSize.Width - pt.X, ScreenSize.Height - pt.Y);
            }

            return pt;
        }

        private void OnInterrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            // The pin is low as long as the screen is being touched.
            // So we still have to poll when the user drags
            if (pinValueChangedEventArgs.PinNumber != _interruptPin)
            {
                return;
            }

            _isPressed = pinValueChangedEventArgs.ChangeType == PinEventTypes.Falling;
            _updateEvent.Set();
        }

        /// <summary>
        /// Returns true if the interrupt pin is set, meaning something is touching the display
        /// </summary>
        /// <returns>True if something presses the display, false if not. This queries the interrupt pin if available. Otherwise, an I2C request to the controller is required.</returns>
        public bool IsPressed()
        {
            if (_gpioController != null)
            {
                return _isPressed;
            }

            // Need to query the device instead
            Span<byte> register = stackalloc byte[1]
            {
                0x02
            };

            Span<byte> result = stackalloc byte[1];

            _i2c.WriteRead(register, result);

            return result[0] != 0;
        }

        private void ReadData()
        {
            lock (_lock)
            {
                // true if real read, not a "come back later"
                _wasRead = false;

                Span<Point> p = stackalloc Point[2];
                p[0] = default;
                p[1] = default;
                byte pts = 0;
                Span<byte> register = stackalloc byte[1]
                {
                    0x02
                };

                if (IsPressed())
                {
                    Span<byte> data = stackalloc byte[11];
                    try
                    {
                        _i2c.WriteRead(register, data);
                    }
                    catch (TimeoutException)
                    {
                        // Try again next time
                        return;
                    }

                    pts = data[0];
                    if (pts > 2)
                    {
                        return;
                    }

                    if (pts > 0)
                    {
                        // Read the data. Never mind trying to read the "weight" and
                        // "size" properties or using the built-in gestures: they
                        // are always set to zero.
                        p[0].X = ((data[1] << 8) | data[2]) & 0x0fff;
                        p[0].Y = ((data[3] << 8) | data[4]) & 0x0fff;
                        if (pts == 2)
                        {
                            p[1].X = ((data[7] << 8) | data[8]) & 0x0fff;
                            p[1].Y = ((data[9] << 8) | data[10]) & 0x0fff;
                        }
                    }

                    if (p[0].X > ScreenSize.Width || p[0].X < 0 || p[0].Y > ScreenSize.Height || p[0].Y < 0
                        || p[1].X > ScreenSize.Width || p[1].X < 0 || p[1].Y > ScreenSize.Height || p[1].Y < 0)
                    {
                        // Invalid data
                        return;
                    }
                }

                if (p[0].X < 0 || p[0].X >= ScreenSize.Width || p[0].Y < 0 || p[1].Y >= ScreenSize.Height)
                {
                    // Drop invalid positions
                    _activeTouches = 0;
                }

                _points[0] = FlipPointIfNeeded(p[0]);
                _points[1] = FlipPointIfNeeded(p[1]);
                _activeTouches = pts;

                _wasRead = true;
            }
        }

        /// <summary>
        /// Gets the primary touch point or null if the screen is not being touched
        /// </summary>
        /// <returns>A point where the first finger is</returns>
        public Point? GetPrimaryTouchPoint()
        {
            ReadData();
            if (!_wasRead)
            {
                return null;
            }

            if (_activeTouches >= 1)
            {
                return _points[0];
            }

            return null;
        }

        /// <summary>
        /// Enables event callback.
        /// This starts an internal thread that will fire the <see cref="Touched"/> and <see cref="Dragging"/> events
        /// </summary>
        public void EnableEvents()
        {
            if (_updateThread != null)
            {
                return;
            }

            _updateThreadActive = true;
            _updateThread = new Thread(UpdateLoop);
            _updateThread.Name = "Touch Controller";
            _updateThread.Start();
        }

        private void UpdateLoop()
        {
            while (_updateThreadActive)
            {
                _updateEvent.WaitOne(_interval);
                if (!_isPressed && _lastActiveTouches == 0)
                {
                    continue;
                }

                ReadData();
                lock (_lock)
                {
                    if (_activeTouches == 0 && _lastActiveTouches == 1 && !_dragging)
                    {
                        // Should not do this within the lock, but call is synchronous anyway, so if it takes to long, we
                        // are blocked either way
                        Touched?.Invoke(this, _lastPoints[0]);
                    }
                    else if (_activeTouches == 0)
                    {
                        _dragging = false;
                        if (_lastActiveTouches == 1)
                        {
                            Dragging?.Invoke(this, new DragEventArgs(false, true, _lastPoints[0], _lastPoints[0]));
                        }
                    }

                    if (_activeTouches == 1 && _lastActiveTouches == 0)
                    {
                        _initialTouchPoint = _points[0];
                    }

                    if (_activeTouches == 1 && _lastActiveTouches == 1)
                    {
                        if (_dragging || Math.Abs(_initialTouchPoint.X - _points[0].X) > TouchSize.Width || Math.Abs(_initialTouchPoint.Y - _points[0].Y) > TouchSize.Height)
                        {
                            Dragging?.Invoke(this, new DragEventArgs(!_dragging, false, _lastPoints[0], _points[0]));
                            _dragging = true;
                        }
                    }

                    if (_activeTouches == 2 && _lastActiveTouches == 2)
                    {
                        int oldXDiff = _lastPoints[0].X - _lastPoints[1].X;
                        int oldYDiff = _lastPoints[0].Y - _lastPoints[1].Y;
                        int oldDiff = (int)Math.Sqrt(oldYDiff * oldYDiff + oldXDiff * oldXDiff);

                        int newXDiff = _points[0].X - _points[1].X;
                        int newYDiff = _points[0].Y - _points[1].Y;
                        int newDiff = (int)Math.Sqrt(newXDiff * newXDiff + newYDiff * newYDiff);
                        if (oldDiff != 0 || newDiff != 0)
                        {
                            Zooming?.Invoke(this, _points, oldDiff, newDiff);
                        }
                    }

                    _lastActiveTouches = _activeTouches;
                    _lastPoints[0] = _points[0];
                    _lastPoints[1] = _points[1];
                }
            }
        }

        /// <summary>
        /// Dispose of this instance and close connections
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateThreadActive = false;
                _updateThread?.Join();
                _updateThread = null;
                if (_interruptPin >= 0 && _gpioController != null)
                {
                    _gpioController.UnregisterCallbackForPinValueChangedEvent(_interruptPin, OnInterrupt);
                    _gpioController.ClosePin(_interruptPin);

                    if (_shouldDispose)
                    {
                        _gpioController.Dispose();
                    }
                }

                _gpioController = null;

                _i2c?.Dispose();
                _i2c = null!;
                _updateEvent?.Dispose();
                _updateEvent = null!;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
