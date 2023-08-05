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
using static Interop;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// A mouse click simulator that uses /dev/uinput for simulating a touch screen (basically a mouse, but with absolute coordinates)
    /// </summary>
    public sealed unsafe class MouseClickSimulatorUInput : IInputDeviceSimulator, IDisposable
    {
        private int _fd;

        /// <summary>
        /// Construct an instance of this class
        /// </summary>
        /// <param name="width">Width of device</param>
        /// <param name="height">Height of device</param>
        public MouseClickSimulatorUInput(int width, int height)
        {
            uinput_setup usetup = default;

            _fd = open("/dev/uinput", Interop.FileOpenFlags.O_WRONLY | Interop.FileOpenFlags.O_NONBLOCK);

            if (_fd <= 0)
            {
                throw new IOException($@"Could not open /dev/uinput. Error code {_fd}. The most likely case is insufficient permissions on '/dev/uinput'. To
fix this, create a file '/etc/udev/rules.d/98-input.rules' with content 'SUBSYSTEM==""input"", PROGRAM=""/bin/sh -c 'chmod 0666 /dev/uinput'""'");
            }

            /* enable mouse button left and relative events */
            ioctlv(_fd, UI_SET_EVBIT, EV_KEY);
            ioctlv(_fd, UI_SET_KEYBIT, BTN_LEFT);
            ioctlv(_fd, UI_SET_KEYBIT, BTN_MIDDLE);
            ioctlv(_fd, UI_SET_KEYBIT, BTN_RIGHT);

            ioctlv(_fd, UI_SET_EVBIT, EV_ABS);
            ioctlv(_fd, UI_SET_ABSBIT, ABS_X);
            ioctlv(_fd, UI_SET_ABSBIT, ABS_Y);

            uinput_abs_setup abssetup = default;
            abssetup.code = ABS_X;
            abssetup.absinfo.minimum = 0;
            abssetup.absinfo.maximum = width;
            abssetup.absinfo.flat = 0;
            abssetup.absinfo.fuzz = 0;
            abssetup.absinfo.value = 0;
            abssetup.absinfo.resolution = 0;
            ioctlv(_fd, UI_ABS_SETUP, new IntPtr(&abssetup));
            abssetup.code = ABS_Y;
            abssetup.absinfo.minimum = 0;
            abssetup.absinfo.maximum = height;
            abssetup.absinfo.flat = 0;
            abssetup.absinfo.fuzz = 0;
            abssetup.absinfo.value = 0;
            abssetup.absinfo.resolution = 0;
            ioctlv(_fd, UI_ABS_SETUP, new IntPtr(&abssetup));

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

            ioctlv(_fd, UI_DEV_SETUP, new IntPtr(&usetup));
            ioctlv(_fd, UI_DEV_CREATE, 0);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MouseClickSimulatorUInput()
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

        /// <summary>
        /// Move the mouse to the given position (absolute)
        /// </summary>
        /// <param name="pt">Point to move the mouse to</param>
        public void MoveMouseTo(Point pt)
        {
            Console.WriteLine($"Moving to {pt.X},{pt.Y}");
            Emit(_fd, EV_ABS, ABS_X, pt.X);
            Emit(_fd, EV_ABS, ABS_Y, pt.Y);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <summary>
        /// Returns nothing useful
        /// </summary>
        public (int X, int Y) GetPosition()
        {
            throw new NotSupportedException(); // Not currently supported by this device
        }

        /// <summary>
        /// Clicks the mouse at the given position.
        /// </summary>
        /// <param name="x">X position of mouse</param>
        /// <param name="y">Y position of mouse</param>
        /// <param name="button">Button to click</param>
        public void Click(int x, int y, MouseButton button)
        {
            MoveTo(x, y);
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
        public void ButtonDown(int x, int y, MouseButton button)
        {
            MoveTo(x, y);
            int btn = GetButtonKeyCode(button);

            Emit(_fd, EV_KEY, btn, 1);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <inheritdoc />
        public void ButtonUp(int x, int y, MouseButton button)
        {
            MoveTo(x, y);
            int btn = GetButtonKeyCode(button);

            Emit(_fd, EV_KEY, btn, 0);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }

        /// <summary>
        /// Closes the device
        /// </summary>
        public void Dispose()
        {
            ioctl(_fd, UI_DEV_DESTROY, 0);
            close(_fd);
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
            int result = write(fd, new IntPtr(&ie), size);
            if (result != size)
            {
                throw new IOException($"Unable to write to input device stream: {Marshal.GetLastWin32Error()}");
            }
        }
}
}
