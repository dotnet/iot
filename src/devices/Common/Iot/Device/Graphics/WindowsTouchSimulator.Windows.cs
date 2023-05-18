// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Simulates a touch device on Windows
    /// </summary>
    public class WindowsTouchSimulator : IInputDeviceSimulator
    {
        /// <summary>
        /// Returns true for this device
        /// </summary>
        public bool AbsoluteCoordinates => true;

        private static void PerformClickWindows(Point pt, MouseButton buttons)
        {
            var mpt = new Interop.MousePoint(pt.X, pt.Y);
            Interop.SetCursorPosition(mpt);
            if ((buttons & MouseButton.Left) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.LeftDown | Interop.MouseEventFlags.Absolute);
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.LeftUp | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Right) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.RightDown | Interop.MouseEventFlags.Absolute);
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.RightUp | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Middle) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.MiddleDown | Interop.MouseEventFlags.Absolute);
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.MiddleUp | Interop.MouseEventFlags.Absolute);
            }
        }

        private static void MouseDownWindows(Point pt, MouseButton buttons)
        {
            var mpt = new Interop.MousePoint(pt.X, pt.Y);
            Interop.SetCursorPosition(mpt);
            if ((buttons & MouseButton.Left) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.LeftDown | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Right) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.RightDown | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Middle) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.MiddleDown | Interop.MouseEventFlags.Absolute);
            }
        }

        private static void MouseUpWindows(Point pt, MouseButton buttons)
        {
            var mpt = new Interop.MousePoint(pt.X, pt.Y);
            Interop.SetCursorPosition(mpt);
            if ((buttons & MouseButton.Left) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.LeftUp | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Right) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.RightUp | Interop.MouseEventFlags.Absolute);
            }

            if ((buttons & MouseButton.Middle) != MouseButton.None)
            {
                Interop.MouseEvent(mpt, Interop.MouseEventFlags.MiddleUp | Interop.MouseEventFlags.Absolute);
            }
        }

        /// <inheritdoc />
        public void MoveTo(int x, int y)
        {
            var mpt = new Interop.MousePoint(x, y);
            Interop.SetCursorPosition(mpt);
        }

        /// <inheritdoc />
        public void Click(int x, int y, MouseButton button)
        {
            MouseDownWindows(new Point(x, y), button);
            MouseUpWindows(new Point(x, y), button);
        }

        /// <inheritdoc />
        public (int X, int Y) GetPosition()
        {
            var pt = Interop.GetCursorPosition();
            return (pt.X, pt.Y);
        }

        /// <inheritdoc />
        public void ButtonDown(int x, int y, MouseButton button)
        {
            MouseDownWindows(new Point(x, y), button);
        }

        /// <inheritdoc />
        public void ButtonUp(int x, int y, MouseButton button)
        {
            MouseUpWindows(new Point(x, y), button);
        }
    }
}
