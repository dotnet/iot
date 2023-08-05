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
using static Iot.Device.Gui.InteropGui;

namespace Iot.Device.Gui
{
    /// <summary>
    /// A mouse click simulator that uses /dev/uinput for simulating a touch screen (basically a mouse, but with absolute coordinates)
    /// </summary>
    internal sealed unsafe class MouseClickSimulatorUInputAbsolute : IPointingDevice, IDisposable
    {
        private int _fd;

        /// <summary>
        /// Construct an instance of this class
        /// </summary>
        /// <param name="width">Width of device</param>
        /// <param name="height">Height of device</param>
        public MouseClickSimulatorUInputAbsolute(int width, int height)
        {
            uinput_setup usetup = default;

            _fd = Interop.open("/dev/uinput", Interop.FileOpenFlags.O_WRONLY | Interop.FileOpenFlags.O_NONBLOCK);

            if (_fd <= 0)
            {
                throw new IOException($@"Could not open /dev/uinput. Error code {_fd}. The most likely case is insufficient permissions on '/dev/uinput'. To
fix this, create a file '/etc/udev/rules.d/98-input.rules' with content 'SUBSYSTEM==""input"", PROGRAM=""/bin/sh -c 'chmod 0666 /dev/uinput'""'");
            }

            /* enable mouse button left and relative events */
            Interop.ioctlv(_fd, UI_SET_EVBIT, EV_KEY);
            Interop.ioctlv(_fd, UI_SET_KEYBIT, BTN_LEFT);
            Interop.ioctlv(_fd, UI_SET_KEYBIT, BTN_MIDDLE);
            Interop.ioctlv(_fd, UI_SET_KEYBIT, BTN_RIGHT);

            Interop.ioctlv(_fd, UI_SET_EVBIT, EV_ABS);
            Interop.ioctlv(_fd, UI_SET_ABSBIT, ABS_X);
            Interop.ioctlv(_fd, UI_SET_ABSBIT, ABS_Y);

            uinput_abs_setup abssetup = default;
            abssetup.code = ABS_X;
            abssetup.absinfo.minimum = 0;
            abssetup.absinfo.maximum = width;
            abssetup.absinfo.flat = 0;
            abssetup.absinfo.fuzz = 0;
            abssetup.absinfo.value = 0;
            abssetup.absinfo.resolution = 0;
            Interop.ioctlv(_fd, UI_ABS_SETUP, new IntPtr(&abssetup));
            abssetup.code = ABS_Y;
            abssetup.absinfo.minimum = 0;
            abssetup.absinfo.maximum = height;
            abssetup.absinfo.flat = 0;
            abssetup.absinfo.fuzz = 0;
            abssetup.absinfo.value = 0;
            abssetup.absinfo.resolution = 0;
            Interop.ioctlv(_fd, UI_ABS_SETUP, new IntPtr(&abssetup));

            usetup.id.bustype = BUS_USB;
            usetup.id.vendor = 0x1234; /* sample vendor */
            usetup.id.product = 0x5678; /* sample product */
            usetup.ff_effects_max = 0;

            // Length must not exceed UINPUT_MAX_NAME_SIZE!
            byte[] name = Encoding.ASCII.GetBytes("dotnet mouse");
            for (int i = 0; i < name.Length; i++)
            {
                usetup.name[i] = name[i];
            }

            Interop.ioctlv(_fd, UI_DEV_SETUP, new IntPtr(&usetup));
            Interop.ioctlv(_fd, UI_DEV_CREATE, 0);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MouseClickSimulatorUInputAbsolute()
        {
            Dispose();
        }

        /// <summary>
        /// Always returns true for this device (this could be easily rewritten to support both, though)
        /// </summary>
        public bool AbsoluteCoordinates => true;

        /// <inheritdoc />
        public void MoveTo(int x, int y)
        {
            MoveMouseTo(new Point(x, y));
        }

        public void MoveTo(Point point)
        {
            MoveMouseTo(point);
        }

        public void MoveBy(int x, int y)
        {
            throw new NotSupportedException("An absolute mouse device cannot perform a relative movement");
        }

        /// <summary>
        /// Move the mouse to the given position (absolute)
        /// </summary>
        /// <param name="pt">Point to move the mouse to</param>
        internal void MoveMouseTo(Point pt)
        {
            Emit(_fd, EV_ABS, ABS_X, pt.X);
            Emit(_fd, EV_ABS, ABS_Y, pt.Y);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <summary>
        /// Returns nothing useful
        /// </summary>
        public Point GetPosition()
        {
            throw new NotSupportedException("Cannot query current mouse position"); // Not currently supported by this device
        }

        /// <summary>
        /// Clicks the mouse at the given position.
        /// </summary>
        /// <param name="button">Button to click</param>
        public void Click(MouseButton button)
        {
            int btn = GetButtonKeyCode(button);

            Emit(_fd, EV_KEY, btn, 1);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
            Emit(_fd, EV_KEY, btn, 0);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        private static int GetButtonKeyCode(MouseButton button)
        {
            int btn = 0;
            switch (button)
            {
                case MouseButton.Left:
                    btn = BTN_LEFT;
                    break;
                case MouseButton.Right:
                    btn = BTN_RIGHT;
                    break;
                case MouseButton.Middle:
                    btn = BTN_MIDDLE;
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

            Emit(_fd, EV_KEY, btn, 1);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <inheritdoc />
        public void ButtonUp(MouseButton button)
        {
            int btn = GetButtonKeyCode(button);

            Emit(_fd, EV_KEY, btn, 0);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <summary>
        /// Closes the device
        /// </summary>
        public void Dispose()
        {
            if (_fd > 0)
            {
                Interop.ioctl(_fd, UI_DEV_DESTROY, 0);
                Interop.close(_fd);
            }

            _fd = 0;
        }

        private void Emit(int fd, int type, int code, int val)
        {
            input_event ie = default;

            ie.time_sec = 0;
            ie.time_usec = 0;
            ie.type = (UInt16)type;
            ie.code = (UInt16)code;
            ie.value = val;
            /* timestamp values below are ignored */

            int size = sizeof(input_event);
            int result = Interop.write(fd, new IntPtr(&ie), size);
            if (result != size)
            {
                throw new IOException($"Unable to write to input device stream: {Marshal.GetLastWin32Error()}");
            }
        }
}
}
