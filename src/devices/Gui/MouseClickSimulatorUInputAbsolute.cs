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
    internal sealed unsafe class MouseClickSimulatorUInputAbsolute : MouseClickSimulatorUInputBase, IPointingDevice, IDisposable
    {
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
        /// Always returns true for this device (this could be easily rewritten to support both, though)
        /// </summary>
        public override bool AbsoluteCoordinates => true;

        /// <inheritdoc />
        public override void MoveTo(int x, int y)
        {
            MoveMouseTo(new Point(x, y));
        }

        public override void MoveTo(Point point)
        {
            MoveMouseTo(point);
        }

        public override void MoveBy(int x, int y)
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
    }
}
