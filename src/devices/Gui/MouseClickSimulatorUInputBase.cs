// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Gui
{
    internal abstract class MouseClickSimulatorUInputBase : IPointingDevice, IDisposable
    {
        protected int _fd;

        protected MouseClickSimulatorUInputBase()
        {
            _fd = 0;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MouseClickSimulatorUInputBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns nothing useful
        /// </summary>
        public Point GetPosition()
        {
            throw new NotSupportedException("Cannot query current mouse position"); // Not currently supported by this device
        }

        public abstract bool AbsoluteCoordinates { get; }
        public abstract void MoveTo(int x, int y);
        public abstract void MoveTo(Point point);
        public abstract void MoveBy(int x, int y);

        /// <summary>
        /// Clicks the mouse at the given position.
        /// </summary>
        /// <param name="button">Button to click</param>
        public void Click(MouseButton button)
        {
            int btn = GetButtonKeyCode(button);

            Emit(_fd, InteropGui.EV_KEY, btn, 1);
            Emit(_fd, InteropGui.EV_SYN, InteropGui.SYN_REPORT, 0);
            Emit(_fd, InteropGui.EV_KEY, btn, 0);
            Emit(_fd, InteropGui.EV_SYN, InteropGui.SYN_REPORT, 0);
        }

        protected static int GetButtonKeyCode(MouseButton button)
        {
            int btn = 0;
            switch (button)
            {
                case MouseButton.Left:
                    btn = InteropGui.BTN_LEFT;
                    break;
                case MouseButton.Right:
                    btn = InteropGui.BTN_RIGHT;
                    break;
                case MouseButton.Middle:
                    btn = InteropGui.BTN_MIDDLE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }

            return btn;
        }

        /// <inheritdoc />
        public void ButtonDown(MouseButton button)
        {
            int btn = GetButtonKeyCode(button);

            Emit(_fd, InteropGui.EV_KEY, btn, 1);
            Emit(_fd, InteropGui.EV_SYN, InteropGui.SYN_REPORT, 0);
        }

        /// <inheritdoc />
        public void ButtonUp(MouseButton button)
        {
            int btn = GetButtonKeyCode(button);

            Emit(_fd, InteropGui.EV_KEY, btn, 0);
            Emit(_fd, InteropGui.EV_SYN, InteropGui.SYN_REPORT, 0);
        }

        protected unsafe void Emit(int fd, int type, int code, int val)
        {
            InteropGui.input_event ie = default;

            ie.time_sec = 0;
            ie.time_usec = 0;
            ie.type = (UInt16)type;
            ie.code = (UInt16)code;
            ie.value = val;
            /* timestamp values below are ignored */

            int size = sizeof(InteropGui.input_event);
            int result = Interop.write(fd, new IntPtr(&ie), size);
            if (result != size)
            {
                throw new IOException($"Unable to write to input device stream: {Marshal.GetLastWin32Error()}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_fd > 0)
            {
                Interop.ioctl(_fd, InteropGui.UI_DEV_DESTROY, 0);
                Interop.close(_fd);
            }

            _fd = 0;
        }

        /// <summary>
        /// Closes the device
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
