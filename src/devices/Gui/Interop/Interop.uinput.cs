// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1307 // Accessible fields should begin with uppercase letter
#pragma warning disable SA1300 // Element should begin with uppercase letter
#pragma warning disable SA1203 //  error SA1203: Constant fields should appear before non-constant fields
namespace Iot.Device.Gui
{
    internal partial class InteropGui
    {
        // Declaration for the use of /dev/uinput
        internal const int UINPUT_VERSION = 5;
        internal const int UINPUT_MAX_NAME_SIZE = 80;
        internal const int UINPUT_IOCTL_BASE = 'U';

        internal const int BUS_PCI = 0x01;
        internal const int BUS_ISAPNP = 0x02;
        internal const int BUS_USB = 0x03;
        internal const int BUS_HIL = 0x04;
        internal const int BUS_BLUETOOTH = 0x05;
        internal const int BUS_VIRTUAL = 0x06;

        internal static readonly int UI_SET_EVBIT = Interop._IOW(UINPUT_IOCTL_BASE, 100, typeof(int));
        internal static readonly int UI_SET_KEYBIT = Interop._IOW(UINPUT_IOCTL_BASE, 101, typeof(int));
        internal static readonly int UI_SET_RELBIT = Interop._IOW(UINPUT_IOCTL_BASE, 102, typeof(int));
        internal static readonly int UI_SET_ABSBIT = Interop._IOW(UINPUT_IOCTL_BASE, 103, typeof(int));
        internal static readonly int UI_SET_MSCBIT = Interop._IOW(UINPUT_IOCTL_BASE, 104, typeof(int));
        internal static readonly int UI_SET_LEDBIT = Interop._IOW(UINPUT_IOCTL_BASE, 105, typeof(int));
        internal static readonly int UI_SET_SNDBIT = Interop._IOW(UINPUT_IOCTL_BASE, 106, typeof(int));
        internal static readonly int UI_SET_FFBIT = Interop._IOW(UINPUT_IOCTL_BASE, 107, typeof(int));

        internal static readonly int UI_DEV_SETUP = Interop._IOW(UINPUT_IOCTL_BASE, 3, typeof(uinput_setup));
        internal static readonly int UI_ABS_SETUP = Interop._IOW(UINPUT_IOCTL_BASE, 4, typeof(uinput_abs_setup));

        internal static readonly int UI_DEV_CREATE = Interop._IO(UINPUT_IOCTL_BASE, 1);
        internal static readonly int UI_DEV_DESTROY = Interop._IO(UINPUT_IOCTL_BASE, 2);

        internal const int EV_SYN = 0x00;
        internal const int EV_KEY = 0x01;
        internal const int EV_REL = 0x02;
        internal const int EV_ABS = 0x03;
        internal const int EV_MSC = 0x04;
        internal const int EV_SW = 0x05;
        internal const int EV_LED = 0x11;
        internal const int EV_SND = 0x12;
        internal const int EV_REP = 0x14;
        internal const int EV_FF = 0x15;
        internal const int EV_PWR = 0x16;
        internal const int EV_FF_STATUS = 0x17;
        internal const int EV_MAX = 0x1f;
        internal const int EV_CNT = (EV_MAX + 1);

        internal const int BTN_MOUSE = 0x110;
        internal const int BTN_LEFT = 0x110;
        internal const int BTN_RIGHT = 0x111;
        internal const int BTN_MIDDLE = 0x112;
        internal const int BTN_SIDE = 0x113;
        internal const int BTN_EXTRA = 0x114;
        internal const int BTN_FORWARD = 0x115;
        internal const int BTN_BACK = 0x116;
        internal const int BTN_TASK = 0x117;

        /*
        * Relative axes
        */

        internal const int REL_X = 0x00;
        internal const int REL_Y = 0x01;
        internal const int REL_Z = 0x02;
        internal const int REL_RX = 0x03;
        internal const int REL_RY = 0x04;
        internal const int REL_RZ = 0x05;
        internal const int REL_HWHEEL = 0x06;
        internal const int REL_DIAL = 0x07;
        internal const int REL_WHEEL = 0x08;
        internal const int REL_MISC = 0x09;

        /*
        * Absolute axes
        */

        internal const int ABS_X = 0x00;
        internal const int ABS_Y = 0x01;
        internal const int ABS_Z = 0x02;
        internal const int ABS_RX = 0x03;
        internal const int ABS_RY = 0x04;
        internal const int ABS_RZ = 0x05;
        internal const int ABS_THROTTLE = 0x06;
        internal const int ABS_RUDDER = 0x07;
        internal const int ABS_WHEEL = 0x08;
        internal const int ABS_GAS = 0x09;
        internal const int ABS_BRAKE = 0x0a;
        internal const int ABS_HAT0X = 0x10;
        internal const int ABS_HAT0Y = 0x11;
        internal const int ABS_HAT1X = 0x12;
        internal const int ABS_HAT1Y = 0x13;
        internal const int ABS_HAT2X = 0x14;
        internal const int ABS_HAT2Y = 0x15;
        internal const int ABS_HAT3X = 0x16;
        internal const int ABS_HAT3Y = 0x17;
        internal const int ABS_PRESSURE = 0x18;
        internal const int ABS_DISTANCE = 0x19;
        internal const int ABS_TILT_X = 0x1a;
        internal const int ABS_TILT_Y = 0x1b;
        internal const int ABS_TOOL_WIDTH = 0x1c;
        internal const int ABS_VOLUME = 0x20;
        internal const int ABS_MISC = 0x28;

        internal const int SYN_REPORT = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal struct input_id
        {
            public UInt16 bustype;
            public UInt16 vendor;
            public UInt16 product;
            public UInt16 version;
        }

        /// <summary>
        /// Structure to set up an input device
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct uinput_setup
        {
            public input_id id;
            public fixed byte name[UINPUT_MAX_NAME_SIZE];
            public UInt32 ff_effects_max;
        }

        /// <summary>
        /// Structure for an event of an input device
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct input_event
        {
            public UInt64 time_sec; // Probably all modern kernels have 64 bit time structure
            public UInt64 time_usec;
            public UInt16 type;
            public UInt16 code;
            public Int32 value;
        }

        /// <summary>
        /// struct input_absinfo - used by EVIOCGABS/EVIOCSABS ioctls
        ///
        /// Note that input core does not clamp reported values to the
        /// [minimum, maximum] limits, such task is left to userspace.
        ///
        /// The default resolution for main axes (ABS_X, ABS_Y, ABS_Z)
        /// is reported in units per millimeter (units/mm), resolution
        /// for rotational axes (ABS_RX, ABS_RY, ABS_RZ) is reported
        /// in units per radian.
        /// When INPUT_PROP_ACCELEROMETER is set the resolution changes.
        /// The main axes (ABS_X, ABS_Y, ABS_Z) are then reported in
        /// units per g (units/g) and in units per degree per second
        /// (units/deg/s) for rotational axes (ABS_RX, ABS_RY, ABS_RZ).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct input_absinfo
        {
            /// <summary>
            /// Latest reported value for the axis
            /// </summary>
            public Int32 value;

            /// <summary>
            /// Minimum value for the axis
            /// </summary>
            public Int32 minimum;

            /// <summary>
            /// Maximum value for the axis
            /// </summary>
            public Int32 maximum;

            /// <summary>
            /// specifies fuzz value that is used to filter noise from
            /// the event stream.
            /// </summary>
            public Int32 fuzz;

            /// <summary>
            /// values that are within this value will be discarded by
            /// joydev interface and reported as 0 instead.
            /// </summary>
            public Int32 flat;

            /// <summary>
            /// specifies resolution for the values reported for the axis.
            /// </summary>
            public Int32 resolution;
        }

        /// <summary>
        /// Structure used to set up an absolute pointing device
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct uinput_abs_setup
        {
            public UInt32 code; /* axis code (original size is Uint16, but due to padding it is actually 32 bits wide)*/

            /* __u16 filler; */
            public input_absinfo absinfo;
        }
    }
}
