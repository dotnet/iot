// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Threading;
using static Interop;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// A mouse simulator that uses the X11 framework.
    /// Not currently working reliably
    /// </summary>
    /// <remarks>Code borrowed from https://gist.github.com/pioz/726474</remarks>
    public class MouseClickSimulatorX11 : IInputDeviceSimulator
    {
        private IntPtr _display;
        private MouseButton _currentButtons;

        /// <summary>
        /// Create an instance of this class.
        /// </summary>
        public MouseClickSimulatorX11()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _display = XOpenDisplay();
                if (_display == IntPtr.Zero)
                {
                    throw new NotSupportedException("Unable to open display - XOpenDisplay failed");
                }
            }

            throw new PlatformNotSupportedException("This simulator requires an X11 compatible system");
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool AbsoluteCoordinates => true;

        private static XButtonEvent GetState(uint button, XButtonEvent ev1)
        {
            // The state is the bitmask of the buttons just prior to the event
            // Therefore it is 0 on a buttonDown event and non-zero on drag and up events
            ev1.state = 0;
            if (button == 1)
            {
                ev1.state = 256;
            }
            else if (button == 2)
            {
                ev1.state = 512;
            }
            else if (button == 3)
            {
                ev1.state = 1024;
            }

            return ev1;
        }

        private void PerformMouseClickLinux(Point pt, MouseButton buttons, bool down, bool up)
        {
            MoveMouseTo(pt);
            if ((buttons & MouseButton.Left) != MouseButton.None)
            {
                PerformMouseClickLinux(1, down, up);
            }

            if ((buttons & MouseButton.Right) != MouseButton.None)
            {
                PerformMouseClickLinux(3, down, up);
            }

            if ((buttons & MouseButton.Middle) != MouseButton.None)
            {
                PerformMouseClickLinux(2, down, up);
            }

            if (down && !up)
            {
                _currentButtons = buttons;
            }

            if (up)
            {
                _currentButtons = MouseButton.None;
            }
        }

        private MouseButton GetMouseCoordinates(out Point pt)
        {
            XButtonEvent ev = default;
            XQueryPointer(_display, XDefaultRootWindow(_display),
                ref ev.root, ref ev.window,
                ref ev.x_root, ref ev.y_root,
                ref ev.x, ref ev.y,
                ref ev.state);

            pt = default;
            pt.X = ev.x;
            pt.Y = ev.y;

            MouseButton mode = MouseButton.None;

            if ((ev.state & 1) != 0)
            {
                mode |= MouseButton.Left;
            }

            if ((ev.state & 2) != 0)
            {
                mode |= MouseButton.Right;
            }

            if ((ev.state & 3) != 0)
            {
                mode |= MouseButton.Middle;
            }

            return mode;
        }

        private void MoveMouseTo(Point pt)
        {
            MoveMouseTo(pt.X, pt.Y);
        }

        private void MoveMouseTo(int x, int y)
        {
            GetMouseCoordinates(out Point pt);
            // XWarpPointer(_display, Window.Zero, Window.Zero, 0, 0, 0, 0, -pt.X, -pt.Y);
            // XWarpPointer(_display, Window.Zero, Window.Zero, 0, 0, 0, 0, x, y);
            // XWarpPointer moves the mouse relative, therefore offset the movement with the current position
            XWarpPointer(_display, Window.Zero, Window.Zero, 0, 0, 0, 0, x - pt.X, y - pt.Y);

            XMotionEvent ev1 = default;
            ev1.type = MotionNotify;
            ev1.subwindow = XDefaultRootWindow(_display);
            while (ev1.subwindow != Window.Zero)
            {
                ev1.window = ev1.subwindow;
                XQueryPointer(_display, ev1.window,
                    ref ev1.root, ref ev1.subwindow,
                    ref ev1.x_root, ref ev1.y_root,
                    ref ev1.x, ref ev1.y,
                    ref ev1.state);
            }

            ev1.state = 0;
            ev1.is_hint = 0;
            if ((_currentButtons & MouseButton.Left) != MouseButton.None)
            {
                ev1.state = 256;
            }

            if ((_currentButtons & MouseButton.Right) != MouseButton.None)
            {
                ev1.state = 1024;
            }

            if ((_currentButtons & MouseButton.Middle) != MouseButton.None)
            {
                ev1.state = 512;
            }

            // Console.WriteLine($"Mouse moving to {ev1.x}, {ev1.y}, state {ev1.state}");
            XSendEvent(_display, ev1.window, true, PointerMotionMask | PointerMotionHintMask | ButtonMotionMask, ref ev1);

            XFlush(_display);
            Thread.Sleep(100);
        }

        private void PerformMouseClickLinux(uint button, bool down, bool up)
        {
            // Create and setting up the event
            Interop.XButtonEvent ev1 = default;

            ev1.button = button;
            ev1.same_screen = true;
            ev1.send_event = 1;
            ev1.root = ev1.subwindow = ev1.window = XDefaultRootWindow(_display);
            Console.WriteLine($"Root window is {GetWindowDescription(_display, ev1.root)}");
            while (ev1.subwindow != Window.Zero)
            {
                ev1.window = ev1.subwindow;
                XQueryPointer(_display, ev1.window,
                ref ev1.root, ref ev1.subwindow,
                ref ev1.x_root, ref ev1.y_root,
                ref ev1.x, ref ev1.y,
                ref ev1.state);
            }

            // Press
            if (down)
            {
                ev1.type = ButtonPress;
                ev1 = GetState(0, ev1);
                Console.WriteLine($"{Environment.TickCount} Mouse down at position {ev1.x}, {ev1.y} state {ev1.state} of window {GetWindowDescription(_display, ev1.window)}");
                if (XSendEvent(_display, Window.Zero, true, 0, ref ev1) == 0)
                {
                    throw new InvalidOperationException("Error sending mouse press event");
                }

                XFlush(_display);
                Thread.Sleep(100);
            }

            if (up)
            {
                // Release
                ev1.type = ButtonRelease;
                ev1 = GetState(button, ev1);

                Console.WriteLine($"{Environment.TickCount} Mouse up at position {ev1.x}, {ev1.y} state {ev1.state} of window {GetWindowDescription(_display, ev1.window)}");
                if (XSendEvent(_display, Window.Zero, true, 0, ref ev1) == 0)
                {
                    throw new InvalidOperationException("Error sending mouse release event");
                }

                XFlush(_display);
                Thread.Sleep(100);
            }
        }

        /// <inheritdoc />
        public void MoveTo(int x, int y)
        {
            MoveMouseTo(x, y);
        }

        /// <inheritdoc />
        public void Click(int x, int y, MouseButton button)
        {
            MoveMouseTo(x, y);
            PerformMouseClickLinux(new Point(x, y), button, true, true);
        }

        /// <inheritdoc />
        public (int X, int Y) GetPosition()
        {
            GetMouseCoordinates(out var pt);
            return (pt.X, pt.Y);
        }

        /// <inheritdoc />
        public void ButtonDown(int x, int y, MouseButton button)
        {
            PerformMouseClickLinux(new Point(x, y), button, true, false);
        }

        /// <inheritdoc />
        public void ButtonUp(int x, int y, MouseButton button)
        {
            PerformMouseClickLinux(new Point(x, y), button, false, true);
        }
    }
}
