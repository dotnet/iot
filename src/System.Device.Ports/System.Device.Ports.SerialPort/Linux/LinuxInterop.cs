// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;
using UnitsNet;
using static System.Net.WebRequestMethods;

/*
    struct termios {
        tcflag_t c_iflag;
        tcflag_t c_oflag;
        tcflag_t c_cflag;
        tcflag_t c_lflag;
        cc_t c_line;
        cc_t c_cc[NCCS];
        speed_t c_ispeed;
        speed_t c_ospeed;
    };

    public const int NCCS 19
    typedef unsigned int tcflag_t;      // uint
    typedef unsigned char cc_t;         // byte
    typedef unsigned int speed_t;       // uint

*/

namespace System.Device.Ports.SerialPort.Linux
{
    internal class LinuxInterop
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Termios
        {
            public uint IFlag;
            public uint OFlag;
            public uint CFlag;
            public uint LFlag;

            public byte Line;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LinuxConstants.NCCS)]
            public byte[] Cc;
            public uint ISpeed; // this is part of the termios2
            public uint OSpeed; // this is part of the termios2
        }

        internal partial class Termios_old
        {
            [Flags]
            internal enum Signals
            {
                None = 0,
                SignalDtr = 1 << 0,
                SignalDsr = 1 << 1,
                SignalRts = 1 << 2,
                SignalCts = 1 << 3,
                SignalDcd = 1 << 4,
                SignalRng = 1 << 5,
                Error = -1,
            }

            internal enum Queue
            {
                AllQueues = 0,
                ReceiveQueue = 1,
                SendQueue = 2,
            }

        }

        /*
        [StructLayout(LayoutKind.Sequential, Size = 104)]
        internal struct Capability
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Driver;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Device;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string BusInfo;

            public uint Version;

            // ????
            // public CapabilityFlags Capabilities;
        }
        */

        /// <summary>
        /// Put the state of FD into *TERMIOS_P
        /// int tcgetattr(int fd, struct termios *termios_p);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcgetattr", SetLastError = true)]
        public static extern int TcGetAttr(int fd, out Termios termios);

        /// <summary>
        /// Set the state of FD to *TERMIOS_P.
        /// Values for OPTIONAL_ACTIONS(TCSA*) are in bits/termios.h
        /// int tcsetattr(int fd, int optional_actions, const struct termios *termios_p);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcsetattr", SetLastError = true)]
        public static extern int TcSetAttr(int fd, uint optionalActions, in Termios termios);

        /// <summary>
        /// Send zero bits on FD
        /// int tcsendbreak(int fd, int duration);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcsendbreak", SetLastError = true)]
        public static extern int TcSendBreak(int fd, int duration);

        /// <summary>
        /// Wait for pending output to be written on FD
        /// int tcdrain(int fd);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcdrain", SetLastError = true)]
        public static extern int TcDrain(int fd);

        /// <summary>
        /// Flush pending data on FD.
        /// Values for QUEUE_SELECTOR(TC{ I,O,IO}FLUSH) are in bits/termios.h
        /// int tcflush(int fd, int queue_selector);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcflush", SetLastError = true)]
        public static extern int TcFlush(int fd, uint queueSelector);

        /// <summary>
        /// Suspend or restart transmission on FD.
        /// Values for ACTION(TC[IO]{ OFF,ON}) are in bits/termios.h
        /// int tcflow(int fd, int action);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "tcflow", SetLastError = true)]
        public static extern int TcFlow(int fd, int action);

        /// <summary>
        /// Set *TERMIOS_P to indicate raw mode
        /// void cfmakeraw(struct termios *termios_p);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfmakeraw", SetLastError = true)]
        public static extern int CfMakeRaw(ref Termios termios);

        /// <summary>
        /// Return the input baud rate stored in *TERMIOS_P
        /// speed_t cfgetispeed(const struct termios *termios_p);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfgetispeed", SetLastError = true)]
        public static extern uint CfGetISpeed(in Termios termios);

        /// <summary>
        /// Return the output baud rate stored in *TERMIOS_P
        /// speed_t cfgetospeed(const struct termios *termios_p);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfgetospeed", SetLastError = true)]
        public static extern uint CfGetOSpeed(in Termios termios);

        /// <summary>
        /// Set the input baud rate stored in *TERMIOS_P to SPEED
        /// int cfsetispeed(struct termios *termios_p, speed_t speed);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfsetispeed", SetLastError = true)]
        public static extern int CfSetISpeed(in Termios termios, uint speed);

        /// <summary>
        /// Set the output baud rate stored in *TERMIOS_P to SPEED
        /// int cfsetospeed(struct termios *termios_p, speed_t speed);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfsetospeed", SetLastError = true)]
        public static extern int CfSetOSpeed(in Termios termios, uint speed);

        /// <summary>
        /// Set both the input and output baud rates in *TERMIOS_OP to SPEED
        /// int cfsetspeed(struct termios *termios_p, speed_t speed);
        /// </summary>
        [DllImport(LinuxConstants.Libc, EntryPoint = "cfsetspeed", SetLastError = true)]
        public static extern int CfSetsSpeed(in Termios termios, uint speed);

        [DllImport(LinuxConstants.Libc, EntryPoint = "ioctl", SetLastError = true)]
        public static extern int Ioctl(int fd, uint Command, [In, Out] ref Termios termios);

        [DllImport(LinuxConstants.Libc, EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoctlRead32(int fd, uint Command, [Out] out uint word);

        [DllImport(LinuxConstants.Libc, EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoctlWrite32(int fd, uint Command, [In] in uint word);

    }

}
