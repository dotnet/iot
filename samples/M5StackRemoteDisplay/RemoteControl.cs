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

        private readonly ScreenCapture _capture;
        private readonly string _nmeaSourceAddress;

        private bool _menuMode;
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

        private List<NmeaDataSet> _dataSets;
        private int _selectedDataSet;

        private bool _forceUpdate;
        private PositionProvider _positionProvider;

        public RemoteControl(Chsc6440? touch, Ili9342 screen, M5ToughPowerControl? powerControl, IPointingDevice deviceSimulator, ScreenCapture capture, string nmeaSourceAddress)
        {
            _touch = touch;
            _screen = screen;
            _powerControl = powerControl;
            _menuMode = false;
            _left = 0;
            _top = 0;
            _scale = 1.0f;
            _screenMode = ScreenMode.Mirror;
            _backLight = ElectricPotential.FromMillivolts(3000);
            _mouseButtonsToPress = MouseButton.None;
            _clickSimulator = deviceSimulator ?? throw new ArgumentNullException(nameof(deviceSimulator));
            _capture = capture;
            _nmeaSourceAddress = nmeaSourceAddress;
            _dataSets = new List<NmeaDataSet>();
            _forceUpdate = true;

            _tcpClient = new NmeaTcpClient("TcpClient", nmeaSourceAddress, 10100);
            _tcpClient.OnNewSequence += OnNewSequence;
            _cache = new SentenceCache(_tcpClient);
            _positionProvider = new PositionProvider(_cache);

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
                if (s.TryGetLastSentence(WaterSpeedAndAngle.Id, out WaterSpeedAndAngle sentence))
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
                if (s.TryGetLastSentence(HeadingTrue.Id, out HeadingTrue sentence))
                {
                    return sentence.Angle;
                }

                return null;
            }, "F1"));

            _dataSets.Add(new NmeaValueDataSet("Depth below Surface", s =>
            {
                if (s.TryGetLastSentence(DepthBelowSurface.Id, out DepthBelowSurface sentence))
                {
                    return sentence.Depth.ToUnit(LengthUnit.Meter);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Engine RPM", s =>
            {
                if (s.TryGetLastDinSentence(SeaSmartEngineFast.HexId, out SeaSmartEngineFast sentence))
                {
                    return sentence.RotationalSpeed.ToUnit(RotationalSpeedUnit.RevolutionPerMinute);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Pitch", s =>
            {
                if (s.TryGetTransducerData("PITCH", out TransducerDataSet set))
                {
                    return Angle.FromDegrees(set.Value);
                }

                return null;
            }));

            _dataSets.Add(new NmeaValueDataSet("Roll", s =>
            {
                if (s.TryGetTransducerData("ROLL", out TransducerDataSet set))
                {
                    return Angle.FromDegrees(set.Value);
                }

                return null;
            }));

            _selectedDataSet = 0;
            _leftMouseMenuBar = BitmapImage.CreateFromFile("images/MenuBarLeftMouse.png");
            _rightMouseMenuBar = BitmapImage.CreateFromFile("images/MenuBarRightMouse.png");
            _defaultMenuBar = BitmapImage.CreateFromFile("images/MenuBar.png");
            _openMenu = BitmapImage.CreateFromFile("images/OpenMenu.png");

            _tcpClient.StartDecode();
        }

        private void OnNewSequence(NmeaSinkAndSource nmeaSinkAndSource, NmeaSentence nmeaSentence)
        {
            // Nothing to do here, handled by the message cache (doing this floods the output)
            // Console.WriteLine($"Received sentence: {nmeaSentence.ToString()}");
        }

        private void OnTouched(object o, Point point)
        {
            Debug.WriteLine($"Touched screen at {point}");
            _powerControl?.Beep(TimeSpan.FromMilliseconds(20));
            // For the coordinates here, see the MenuBar.png file
            if (_menuMode && point.Y < 100)
            {
                if (point.X > 222)
                {
                    _menuMode = false;
                    _screenMode = ScreenMode.Mirror;
                    Debug.WriteLine("Changed to mirror mode");
                }
                else if (point.Y < 50 && point.X > 100 && point.X < 160)
                {
                    _screenMode = ScreenMode.Battery;
                    Debug.WriteLine("Changed to battery status mode");
                    _mouseButtonsToPress = MouseButton.None;
                    _menuMode = false;
                }
                else if (point.Y < 50 && point.X >= 160 && point.X < 220)
                {
                    _screenMode = ScreenMode.NmeaValue;
                    Debug.WriteLine("Changed to NMEA display mode");
                    _mouseButtonsToPress = MouseButton.None;
                    _forceUpdate = true;
                    _menuMode = false;
                }
                else if (point.Y > 50 && point.X > 100 && point.X < 160)
                {
                    _scale /= 1.1f;
                    Debug.WriteLine($"Changed scale to {_scale}");
                }
                else if (point.Y > 50 && point.X > 160 && point.X < 220)
                {
                    _scale *= 1.1f;
                    Debug.WriteLine($"Changed scale to {_scale}");
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

                    Debug.WriteLine($"Mouse mode: {_mouseButtonsToPress}");
                }
            }
            else
            {
                if (point.X > _screen.ScreenWidth - 30 && point.Y < 30)
                {
                    _menuMode = true;
                    return;
                }

                if (_screenMode == ScreenMode.Mirror)
                {
                    if (_mouseButtonsToPress != MouseButton.None)
                    {
                        var pt = ToAbsoluteScreenPosition(point);
                        _clickSimulator.MoveTo(pt.X, pt.Y);
                    }
                }
                else if (_screenMode == ScreenMode.NmeaValue)
                {
                    _selectedDataSet = (_selectedDataSet + 1) % _dataSets.Count;
                    _forceUpdate = true;
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
            if (_powerControl != null)
            {
                _powerControl.SetLcdVoltage(_backLight);
            }
        }

        public void DisplayFeatures()
        {
            StartupDisplay();

            bool abort = false;
            Point dragBegin = Point.Empty;

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

            while (!abort)
            {
                Stopwatch sw = Stopwatch.StartNew();
                KeyboardControl(ref abort);

                switch (_screenMode)
                {
                    case ScreenMode.Mirror:
                        DrawScreenContents(backBuffer, _capture, _scale, ref _left, ref _top);
                        break;
                    case ScreenMode.Battery:
                        backBuffer.Clear();
                        DrawPowerStatus(backBuffer);
                        break;
                    case ScreenMode.NmeaValue:
                        if (!DrawNmeaValue(backBuffer, _menuMode || _forceUpdate))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        _forceUpdate = false;
                        break;
                    default:
                        _screen.ClearScreen();
                        break;
                }

                if (_menuMode)
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
                else
                {
                    // Draw the "open menu here" icon over the top right of the screen.
                    backBuffer.GetDrawingApi().DrawImage(_openMenu, new Rectangle(0, 0, _openMenu.Width, _openMenu.Height), new Rectangle(_screen.ScreenWidth - _openMenu.Width, 0, _openMenu.Width, _openMenu.Height));
                }

                _screen.DrawBitmap(backBuffer);
                double fps = 0;
                if (_screen is Ili9341 ili)
                {
                    fps = ili.Fps;
                }
                Console.WriteLine($"\rFPS: {fps}");
                // This typically happens if nothing needs to be done (the screen didn't change)
                if (fps > 10)
                {
                    Thread.Sleep(100);
                }
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
                bmp.Dispose();
            }
        }

        private bool DrawNmeaValue(BitmapImage targetBuffer, bool force)
        {
            var data = _dataSets[_selectedDataSet];
            if (data.Update(_cache, 1E-2) || force)
            {
                targetBuffer.Clear(Color.White);
                using var g = targetBuffer.GetDrawingApi();
                string font = GetDefaultFontName();
                g.DrawText(data.Value, font, 110, Color.Blue, new Point(20, 30));
                g.DrawText(data.Name, font, 20, Color.Blue, new Point(10, 5));
                g.DrawText(data.Unit, font, 20, Color.Blue, new Point(_screen.ScreenWidth / 2, _screen.ScreenHeight - 33));

                return true;
            }
            else
            {
                targetBuffer.Clear(Color.White);
            }

            return false;
        }

        private void StartupDisplay()
        {
            using var image = BitmapImage.CreateFromFile(@"images/Landscape.png");
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

            if (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            Thread.Sleep(500);
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
            _tcpClient.StopDecode();
            _tcpClient.Dispose();
        }
    }
}
