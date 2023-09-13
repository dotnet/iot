// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Gui
{
    /// <summary>
    /// Simulates a touch device on Windows
    /// </summary>
    internal sealed class WindowsMouseSimulator : IPointingDevice
    {
        /// <summary>
        /// Returns true for this device
        /// </summary>
        public bool AbsoluteCoordinates => true;

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
        public void MoveTo(Point point)
        {
            MoveTo(point.X, point.Y);
        }

        public void MoveBy(int x, int y)
        {
            var pt = GetPosition();
            var newPt = new Point(pt.X + x, pt.Y + y);
            MoveTo(newPt.X, newPt.Y);
        }

        /// <inheritdoc />
        public void Click(MouseButton button)
        {
            var pt = GetPosition();
            MouseDownWindows(pt, button);
            MouseUpWindows(pt, button);
        }

        /// <inheritdoc />
        public Point GetPosition()
        {
            var pt = Interop.GetCursorPosition();
            return new Point(pt.X, pt.Y);
        }

        /// <inheritdoc />
        public void ButtonDown(MouseButton button)
        {
            var pt = GetPosition();
            MouseDownWindows(pt, button);
        }

        /// <inheritdoc />
        public void ButtonUp(MouseButton button)
        {
            var pt = GetPosition();
            MouseUpWindows(pt, button);
        }

        public void Dispose()
        {
        }
    }
}
