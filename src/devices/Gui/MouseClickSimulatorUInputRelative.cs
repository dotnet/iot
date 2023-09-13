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
    /// A mouse click simulator that uses /dev/uinput for simulating a mouse with relative coordinates
    /// </summary>
    internal sealed unsafe class MouseClickSimulatorUInputRelative : MouseClickSimulatorUInputBase, IPointingDevice, IDisposable
    {
        /// <summary>
        /// Construct an instance of this class
        /// </summary>
        public MouseClickSimulatorUInputRelative()
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

            Interop.ioctlv(_fd, UI_SET_EVBIT, EV_REL);
            Interop.ioctlv(_fd, UI_SET_RELBIT, REL_X);
            Interop.ioctlv(_fd, UI_SET_RELBIT, REL_Y);

            usetup.id.bustype = BUS_USB;
            usetup.id.vendor = 0x1234; /* sample vendor */
            usetup.id.product = 0x5679; /* sample product */
            usetup.ff_effects_max = 0;

            // Length must not exceed UINPUT_MAX_NAME_SIZE!
            byte[] name = Encoding.ASCII.GetBytes("dotnet mouse2");
            for (int i = 0; i < name.Length; i++)
            {
                usetup.name[i] = name[i];
            }

            Interop.ioctlv(_fd, UI_DEV_SETUP, new IntPtr(&usetup));
            Interop.ioctlv(_fd, UI_DEV_CREATE, 0);
        }

        /// <summary>
        /// Always returns false for this device
        /// </summary>
        public override bool AbsoluteCoordinates => false;

        /// <inheritdoc />
        public override void MoveTo(int x, int y)
        {
            throw new NotSupportedException("A relative mouse device cannot perform an absolute movement");
        }

        public override void MoveTo(Point point)
        {
            throw new NotSupportedException("A relative mouse device cannot perform an absolute movement");
        }

        public override void MoveBy(int x, int y)
        {
            Emit(_fd, EV_REL, REL_X, x);
            Emit(_fd, EV_REL, REL_Y, y);
            Emit(_fd, EV_SYN, SYN_REPORT, 0);
        }
    }
}
