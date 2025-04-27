// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Axp192;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Gui;
using Iot.Device.Ili934x;
using Iot.Device.M5Stack;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Ili934x.Samples
{
    internal sealed class RemoteControl : IDisposable
    {
        // Note: Owner of these is the outer class
        private readonly Chsc6440? _touch;
        private readonly GraphicDisplay _screen;
        private readonly M5ToughPowerControl? _powerControl;

        private readonly BitmapImage _defaultMenuBar;
        private readonly BitmapImage _leftMouseMenuBar;
        private readonly BitmapImage _rightMouseMenuBar;
        private readonly BitmapImage _openMenu;
        private readonly BitmapImage _zoomIn;
        private readonly BitmapImage _zoomOut;

        private readonly ScreenCapture _capture;
        private readonly Arguments _commandLine;

        private bool _overlayMenu;
        private bool _nightMode;
        private float _left;
        private float _top;
        private float _scale;
        private ElectricPotential _backLight;
        private ScreenMode _screenMode;
        private MouseButton _mouseButtonsToPress; // The mouse button(s) to simulate on a touch event
        private Point _lastDragBegin;
        private IPointingDevice _clickSimulator;
        private NmeaTcpClient _tcpClient;
        private SentenceCache _cache;
        private AutoResetEvent _waitForClick;
        private List<NmeaDataSet> _dataSets;
        private int _selectedDataSet;

        private bool _forceUpdate;
        private PositionProvider _positionProvider;
        private AutopilotController _autopilotController;
        private MessageRouter _messageRouter;

        private List<MenuItem> _menuItems;
        private MenuItem? _menuItemSelected;

        public RemoteControl(Chsc6440? touch, Ili9342 screen, M5ToughPowerControl? powerControl, IPointingDevice deviceSimulator, ScreenCapture capture, Arguments commandLine)
        {
            _touch = touch;
            _screen = screen;
            _powerControl = powerControl;
            _overlayMenu = false;
            _left = 0;
            _top = 0;
            _scale = 1.0f;
            _waitForClick = new AutoResetEvent(false);
            _screenMode = ScreenMode.Mirror;
            _backLight = ElectricPotential.FromMillivolts(3000);
            _mouseButtonsToPress = MouseButton.None;
            _clickSimulator = deviceSimulator ?? throw new ArgumentNullException(nameof(deviceSimulator));
            _capture = capture;
            _commandLine = commandLine;
            _dataSets = new List<NmeaDataSet>();
            _forceUpdate = true;
            _menuItemSelected = null;
            _menuItems = new List<MenuItem>();
            _nightMode = false;
            CreateMenuItems();

            _messageRouter = new MessageRouter(new LoggingConfiguration());
            _tcpClient = new NmeaTcpClient("TcpClient", _commandLine.NmeaServer, _commandLine.NmeaPort);
            _tcpClient.OnNewSequence += OnNewSequence;
            _cache = new SentenceCache(_messageRouter);
            _positionProvider = new PositionProvider(_cache);

            _autopilotController = new AutopilotController(_messageRouter, _messageRouter, _cache);

            _messageRouter.AddEndPoint(_tcpClient);

            // The local sink gets everything from the server
            _messageRouter.AddFilterRule(new FilterRule(_tcpClient.InterfaceName, TalkerId.Any, SentenceId.Any, new List<string>() { _messageRouter.InterfaceName },
                false, true));
            // Anything from the local sink (typically output from the Autopilot controller) is only cached for later reuse
            _messageRouter.AddFilterRule(new FilterRule(_messageRouter.InterfaceName, TalkerId.ElectronicChartDisplayAndInformationSystem, SentenceId.Any, 
                new List<string>(), (source, destination, before) =>
                {
                    _cache.Add(before);
                    return null;
                }, true, false));

            _messageRouter.StartDecode();

            _dataSets.Add(new NmeaValueDataSet("Speed over Ground", s =>
            {
                if (_positionProvider.TryGetCurrentPosition(out _, out _, out Speed sog, out _))
                {
                    return sog.ToUnit(SpeedUnit.Knot);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Speed trough water", s =>
            {
                if (s.TryGetLastSentence(WaterSpeedAndAngle.Id, out WaterSpeedAndAngle? sentence))
                {
                    return sentence.Speed.ToUnit(SpeedUnit.Knot);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Course over Ground", s =>
            {
                if (_positionProvider.TryGetCurrentPosition(out _, out Angle track, out _, out _))
                {
                    return track;
                }

                return null;
            }, "F1"));

            _dataSets.Add(new NmeaValueDataSet("Heading", s =>
            {
                if (s.TryGetLastSentence(HeadingTrue.Id, out HeadingTrue? sentence))
                {
                    return sentence.Angle;
                }

                return null;
            }, "F1"));

            _dataSets.Add(new NmeaValueDataSet("Depth below Surface", s =>
            {
                if (s.TryGetLastSentence(DepthBelowSurface.Id, out DepthBelowSurface? sentence))
                {
                    return sentence.Depth.ToUnit(LengthUnit.Meter);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Engine RPM", s =>
            {
                if (s.TryGetLastDinSentence(SeaSmartEngineFast.HexId, out SeaSmartEngineFast? sentence))
                {
                    return sentence.RotationalSpeed.ToUnit(RotationalSpeedUnit.RevolutionPerMinute);
                }

                return null;
            }, "F0"));

            _dataSets.Add(new NmeaValueDataSet("Pitch", s =>
            {
                if (s.TryGetTransducerData("PITCH", out TransducerDataSet? set))
                {
                    return Angle.FromDegrees(set.Value);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Roll", s =>
            {
                if (s.TryGetTransducerData("ROLL", out TransducerDataSet? set))
                {
                    return Angle.FromDegrees(set.Value);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Distance to WP", s =>
            {
                if (_cache.TryGetLastSentence(RecommendedMinimumNavToDestination.Id, out RecommendedMinimumNavToDestination? rmb))
                {
                    return rmb.DistanceToWayPoint;
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Est Time to WP", s =>
            {
                if (_cache.TryGetLastSentence(RecommendedMinimumNavToDestination.Id, out RecommendedMinimumNavToDestination? rmb))
                {
                    if (UnitMath.Abs(rmb.ApproachSpeed.GetValueOrDefault(Speed.Zero)) < Speed.FromKnots(0.1))
                    {
                        return Duration.FromHours(99);
                    }
                    return rmb.DistanceToWayPoint / rmb.ApproachSpeed;
                }

                return null;
            }));

            _selectedDataSet = 0;
            _leftMouseMenuBar = BitmapImage.CreateFromFile("images/MenuBarLeftMouse.png");
            _rightMouseMenuBar = BitmapImage.CreateFromFile("images/MenuBarRightMouse.png");
            _defaultMenuBar = BitmapImage.CreateFromFile("images/MenuBar.png");
            _openMenu = BitmapImage.CreateFromFile("images/OpenMenu.png");
            _zoomIn = BitmapImage.CreateFromFile("images/ZoomPlus.png");
            _zoomOut = BitmapImage.CreateFromFile("images/ZoomMinus.png");

            _tcpClient.StartDecode();
        }

        private void OnNewSequence(NmeaSinkAndSource nmeaSinkAndSource, NmeaSentence nmeaSentence)
        {
            // Nothing to do here, handled by the message cache (doing this floods the output)
            // Console.WriteLine($"Received sentence: {nmeaSentence.ToString()}");
        }

        private void MirrorModeOnTouched(Point point)
        {
            if (_overlayMenu)
            {
                if (point.Y >= 100)
                {
                    _overlayMenu = false;
                }
                else if (point.X > 222)
                {
                    _overlayMenu = false;
                    _screenMode = ScreenMode.NmeaValue;
                    _forceUpdate = true;
                }
                else if (point.X < 100)
                {
                    if (_mouseButtonsToPress == MouseButton.Left)
                    {
                        _mouseButtonsToPress = MouseButton.Right;
                    }
                    else if (_mouseButtonsToPress == MouseButton.Right)
                    {
                        _mouseButtonsToPress = MouseButton.None;
                    }
                    else
                    {
                        _mouseButtonsToPress = MouseButton.Left;
                    }

                    Console.WriteLine($"Mouse mode: {_mouseButtonsToPress}");
                }
                else
                {
                    _overlayMenu = false;
                    _menuItemSelected = null;
                    _screenMode = ScreenMode.MainMenu;
                    _forceUpdate = true;
                }
            }
            else
            {
                if (OpenOverlayClicked(point))
                {
                    _overlayMenu = true;
                    return;
                }

                if (point.Y < _zoomOut.Height && point.X < _zoomOut.Width)
                {
                    _scale = _scale / 1.5f;
                }
                else if (point.Y < _zoomOut.Height && point.X < 2 * _zoomOut.Width)
                {
                    _scale = _scale * 1.5f;
                }
                else if (_mouseButtonsToPress != MouseButton.None)
                {
                    var pt = ToAbsoluteScreenPosition(point);
                    _clickSimulator.MoveTo(pt.X, pt.Y);
                    _clickSimulator.Click(_mouseButtonsToPress);
                }
            }
        }

        private bool OpenOverlayClicked(Point point)
        {
            return point.X > _screen.ScreenWidth - 30 && point.Y < 30;
        }

        private void OnTouched(object o, Point point)
        {
            Console.WriteLine($"Touched screen at {point}");
            _powerControl?.Beep(TimeSpan.FromMilliseconds(20));
            _waitForClick.Set();
            // For the coordinates here, see the MenuBar.png file
            if (_screenMode == ScreenMode.Mirror)
            {
                MirrorModeOnTouched(point);
                return;
            }
            if (_screenMode == ScreenMode.NmeaValue)
            {
                if (OpenOverlayClicked(point))
                {
                    _screenMode = ScreenMode.Mirror;
                    return;
                }
                if (point.X > _screen.ScreenWidth / 2)
                {
                    _selectedDataSet = (_selectedDataSet + 1) % _dataSets.Count;
                    _forceUpdate = true;
                }
                else if (point.X < _screen.ScreenWidth / 2)
                {
                    _selectedDataSet = (_selectedDataSet) > 0 ? _selectedDataSet - 1 : _dataSets.Count - 1;
                    _forceUpdate = true;
                }
            }

            if (_screenMode == ScreenMode.Battery)
            {
                if (OpenOverlayClicked(point))
                {
                    _screenMode = ScreenMode.Mirror;
                    return;
                }
            }

            if (_screenMode == ScreenMode.MainMenu)
            {
                foreach (var item in _menuItems)
                {
                    if (item.Rectangle.Contains(point))
                    {
                        _menuItemSelected = item;
                        item.OnTouched();
                        _menuItemSelected = null;
                        return;
                    }
                }
            }
        }

        private (int X, int Y) ToAbsoluteScreenPosition(Point point)
        {
            return ((int)((point.X + _left) / _scale), (int)((point.Y + _top) / _scale));
        }

        private void OnDragging(object o, DragEventArgs e)
        {
            if (_screenMode != ScreenMode.Mirror)
            {
                return;
            }

            if (_mouseButtonsToPress == MouseButton.Left)
            {
                var pos = ToAbsoluteScreenPosition(e.CurrentPoint);
                _clickSimulator.MoveTo(pos.X, pos.Y);
                if (e.IsDragBegin)
                {
                    _clickSimulator.ButtonDown(_mouseButtonsToPress);
                    _lastDragBegin = e.CurrentPoint;
                }

                _clickSimulator.MoveTo(pos.X, pos.Y);
                if (e.IsDragEnd)
                {
                    _clickSimulator.ButtonUp(_mouseButtonsToPress);
                    _lastDragBegin = new Point(99999, 99999); // Outside
                }

                return;
            }

            var (xdiff, ydiff) = (e.LastPoint.X - e.CurrentPoint.X, e.LastPoint.Y - e.CurrentPoint.Y);
            _left += xdiff * _scale;
            _top += ydiff * _scale;
            Debug.WriteLine($"Dragging at {e.CurrentPoint.X}/{e.CurrentPoint.Y} by {xdiff}/{ydiff}.");
            if (e.IsDragBegin)
            {
                _lastDragBegin = e.LastPoint;
            }
            else if (e.IsDragEnd)
            {
                _lastDragBegin = new Point(99999, 99999); // Outside}
            }
        }

        public void IncreaseBrightness()
        {
            _backLight = _backLight + ElectricPotential.FromMillivolts(200);
            if (_powerControl != null)
            {
                _powerControl.SetLcdVoltage(_backLight);
            }
        }

        public void DecreaseBrightness()
        {
            _backLight = _backLight - ElectricPotential.FromMillivolts(200);
            // Don't go down until the backlight switches off (won't be able to recover)
            _backLight = UnitsNet.UnitMath.Clamp(_backLight, ElectricPotential.FromMillivolts(1800), ElectricPotential.FromMillivolts(3300));
            if (_powerControl != null)
            {
                _powerControl.SetLcdVoltage(_backLight);
            }
        }

        public void Run()
        {
            StartupDisplay();

            bool abort = false;

            var backBuffer = _screen.GetBackBufferCompatibleImage();

            if (_touch != null)
            {
                _touch.Touched += OnTouched;

                _touch.Dragging += OnDragging;

                _touch.Zooming += (o, points, oldDiff, newDiff) =>
                {
                    float scaleChange = (float)oldDiff / newDiff;
                    if (scaleChange != 0)
                    {
                        _scale = _scale / scaleChange;
                    }
                };
            }

            TimeSpan minTimeBetweenFrames = TimeSpan.FromMilliseconds(100);
            _menuItemSelected = null;
            CreateMenuItems();
            while (!abort)
            {
                Stopwatch sw = Stopwatch.StartNew();
                KeyboardControl(ref abort);

                switch (_screenMode)
                {
                    case ScreenMode.Mirror:
                        DrawScreenContents(backBuffer, _capture, _scale, ref _left, ref _top);
                        minTimeBetweenFrames = TimeSpan.FromMilliseconds(100);
                        break;
                    case ScreenMode.Battery:
                        backBuffer.Clear();
                        DrawPowerStatus(backBuffer);
                        minTimeBetweenFrames = TimeSpan.FromSeconds(1);
                        break;
                    case ScreenMode.NmeaValue:
                        DrawNmeaValue(backBuffer, _forceUpdate);
                        minTimeBetweenFrames = TimeSpan.FromMilliseconds(500);
                        _forceUpdate = false;
                        break;
                    case ScreenMode.MainMenu:
                        DrawMenuEntries(backBuffer, _menuItemSelected);
                        minTimeBetweenFrames = TimeSpan.FromMilliseconds(2000);
                        break;
                    default:
                        _screen.ClearScreen();
                        break;
                }

                if (_overlayMenu)
                {
                    BitmapImage bm = _defaultMenuBar;
                    if (_mouseButtonsToPress == MouseButton.Left)
                    {
                        bm = _leftMouseMenuBar;
                    }
                    else if (_mouseButtonsToPress == MouseButton.Right)
                    {
                        bm = _rightMouseMenuBar;
                    }

                    backBuffer.GetDrawingApi().DrawImage(bm, 0, 0);
                }
                else if (_screenMode != ScreenMode.MainMenu)
                {
                    // Draw the "open menu here" icon over the top right of the screen.
                    backBuffer.GetDrawingApi().DrawImage(_openMenu, new Rectangle(0, 0, _openMenu.Width, _openMenu.Height), new Rectangle(_screen.ScreenWidth - _openMenu.Width, 0, _openMenu.Width, _openMenu.Height));
                }

                if (_commandLine.FlipScreen)
                {
                    var rotated = backBuffer.Rotate(180);
                    _screen.DrawBitmap(rotated);
                    rotated.Dispose();
                }
                else
                {
                    _screen.DrawBitmap(backBuffer);
                }

                double fps = 0;
                if (_screen is Ili9341 ili)
                {
                    fps = ili.Fps;
                    Console.Write($"\rFPS: {fps:F1}     ");
                }
                _waitForClick.WaitOne(minTimeBetweenFrames);
            }
        }

        private void CreateMenuItems()
        {
            _menuItems.Clear();
            int itemHeight = 36;
            _menuItems.Add(new MenuItem(() => _nightMode ? "Clear Night mode" : "Set Night Mode", new Rectangle(0, 0, _screen.ScreenWidth - 1, itemHeight), false, () => { _nightMode = !_nightMode; }));
            _menuItems.Add(new MenuItem(() => "Increase Brightness", new Rectangle(0, itemHeight, _screen.ScreenWidth - 1, itemHeight), false, IncreaseBrightness));
            _menuItems.Add(new MenuItem(() => "Decrease Brightness", new Rectangle(0, 2 * itemHeight, _screen.ScreenWidth - 1, itemHeight), false, DecreaseBrightness));
            _menuItems.Add(new MenuItem(() => "Exit Menu", new Rectangle(0, _screen.ScreenHeight - itemHeight - 1, _screen.ScreenWidth - 1, itemHeight), false, () => { _screenMode = ScreenMode.Mirror; }));
        }

        private void DrawMenuEntries(BitmapImage backBuffer, MenuItem? selectedItem)
        {
            backBuffer.Clear();
            foreach (var item in _menuItems)
            {
                string font = GetDefaultFontName();
                backBuffer.GetDrawingApi().DrawRectangle(item.Rectangle, Color.FromArgb(67, 188, 228));
                if (selectedItem == item)
                {
                    backBuffer.GetDrawingApi().FillRectangle(item.Rectangle, Color.LightYellow);
                }

                backBuffer.GetDrawingApi().DrawText(item.Text(), font, item.Rectangle.Height - 6, Color.Blue, new Point(0, item.Rectangle.Y + 1));
            }
        }

        private void KeyboardControl(ref bool abort)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Escape:
                        abort = true;
                        break;
                    case ConsoleKey.RightArrow:
                        _left += 10;
                        break;
                    case ConsoleKey.DownArrow:
                        _top += 10;
                        break;
                    case ConsoleKey.LeftArrow:
                        _left -= 10;
                        break;
                    case ConsoleKey.UpArrow:
                        _top -= 10;
                        break;
                    case ConsoleKey.Add:
                        _scale *= 1.1f;
                        break;
                    case ConsoleKey.Subtract:
                        _scale /= 1.1f;
                        break;
                    case ConsoleKey.Insert:
                        IncreaseBrightness();
                        break;
                    case ConsoleKey.Delete:
                        DecreaseBrightness();
                        break;
                }
            }
        }

        private void DrawScreenContents(BitmapImage buffer, ScreenCapture capture, float scale, ref float left, ref float top)
        {
            var bmp = capture.GetScreenContents();
            if (bmp != null)
            {
                using var resizedBitmap = bmp.Resize(new Size((int)(bmp.Width * scale), (int)(bmp.Height * scale)));
                var pt = new Point((int)left, (int)top);
                var rect = new Rectangle(0, 0, _screen.ScreenWidth, _screen.ScreenHeight);
                Converters.AdjustImageDestination(resizedBitmap, ref pt, ref rect);
                left = pt.X;
                top = pt.Y;

                buffer.GetDrawingApi().DrawImage(resizedBitmap, -pt.X, -pt.Y);
                buffer.GetDrawingApi().DrawImage(_zoomOut, 0, 0);
                buffer.GetDrawingApi().DrawImage(_zoomIn, _zoomOut.Width, 0);
                bmp.Dispose();
            }
        }

        private bool DrawNmeaValue(BitmapImage targetBuffer, bool force)
        {
            const int leftOffset = 20;
            var data = _dataSets[_selectedDataSet];
            if (data.Update(_cache, 1E-2) || force)
            {
                if (_nightMode)
                {
                    targetBuffer.Clear(Color.Black);
                }
                else
                {
                    targetBuffer.Clear(Color.White);
                }

                using var g = targetBuffer.GetDrawingApi();
                string font = GetDefaultFontName();
                var size = g.MeasureText(data.Value, font, 110);
                int actualSize = 110;
                if (size.Width + leftOffset > _screen.ScreenWidth)
                {
                    float ratio = size.Width / (_screen.ScreenWidth - leftOffset);
                    actualSize = (int)(actualSize / ratio);
                }
                g.DrawText(data.Value, font, actualSize, Color.Blue, new Point(20, 30));
                g.DrawText(data.Name, font, 30, Color.Blue, new Point(10, 5));
                g.DrawText(data.Unit, font, 30, Color.Blue, new Point(_screen.ScreenWidth / 2, _screen.ScreenHeight - 33));
                // Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {data.Name} {data.Value} {data.Unit}");
                return true;
            }

            return false;
        }

        private void StartupDisplay()
        {
            var image = BitmapImage.CreateFromFile(@"images/Landscape.png");
            if (_commandLine.FlipScreen)
            {
                image = image.Rotate(180);
            }
            using var backBuffer = _screen.GetBackBufferCompatibleImage();
            for (int i = 1; i < 20; i++)
            {
                float factor = i / 10.0f;
                if (Console.KeyAvailable || (_touch != null && _touch.IsPressed()))
                {
                    break;
                }

                IGraphics api = backBuffer.GetDrawingApi();
                Rectangle newRect = Rectangle.Empty;
                newRect.Width = (int)(image.Width * factor);
                newRect.Height = (int)(image.Height * factor);
                newRect.X = (backBuffer.Width / 2) - (newRect.Width / 2);
                newRect.Y = (backBuffer.Height / 2) - (newRect.Height / 2);
                api.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), newRect);

                _screen.DrawBitmap(backBuffer);
                Thread.Sleep(100);
            }

            image.Dispose();
            if (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        private String GetDefaultFontName()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return "Arial";
            }
            else
            {
                return "Liberation Sans";
            }
        }

        private void DrawPowerStatus(BitmapImage buffer)
        {
            if (_powerControl != null)
            {
                var pc = _powerControl.GetPowerControlData();
                var font = GetDefaultFontName();
                using var g = buffer.GetDrawingApi();
                g.DrawText(pc.ToString(), font, 18, Color.Blue, new Point(0, 10));
            }
        }

        public void Dispose()
        {
            _messageRouter.Dispose();
            _autopilotController.Dispose();
            _tcpClient.StopDecode();
            _tcpClient.Dispose();
        }

        private sealed record MenuItem(Func<string> Text, Rectangle Rectangle, bool Selected, Action OnTouched)
        {
        }
    }
}
