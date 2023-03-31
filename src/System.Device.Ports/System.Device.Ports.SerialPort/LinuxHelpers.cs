// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

/*
    Termios man page: https://linux.die.net/man/3/termios
    Code browser: https://codebrowser.dev/glibc/glibc/termios/termios.h.html

    struct termios {
        tcflag_t c_iflag;
        tcflag_t c_oflag;
        tcflag_t c_cflag;
        tcflag_t c_lflag;
        cc_t c_cc[NCCS];
        speed_t c_ispeed;
        speed_t c_ospeed;
    };

    #define NCCS 19
    typedef unsigned int tcflag_t;      // uint
    typedef unsigned char cc_t;         // byte
    typedef unsigned int speed_t;       // uint

    Return value:
    - Success: 0
    - Failure: -1 (errno)
    - cfgetispeed() and cfgetospeed(): baud rate
*/

namespace System.Device.Ports.SerialPort
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Termios
    {
        public uint IFlag;
        public uint OFlag;
        public uint CFlag;
        public uint LFlag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LinuxHelpers.NCCS)]
        public byte[] Cc;
        public uint ISpeed;
        public uint OSpeed;
    }

    internal class LinuxHelpers
    {
        internal const string Libc = "libc";
        internal const int NCCS = 19;

        internal static partial class TermiosInterop
        {
            /// <summary>
            /// Put the state of FD into *TERMIOS_P
            /// int tcgetattr(int fd, struct termios *termios_p);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcgetattr", SetLastError = true)]
            public static extern int TcGetAttr(int fd, out Termios termios);

            /// <summary>
            /// Set the state of FD to *TERMIOS_P.
            /// Values for OPTIONAL_ACTIONS(TCSA*) are in bits/termios.h
            /// int tcsetattr(int fd, int optional_actions, const struct termios *termios_p);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcsetattr", SetLastError = true)]
            public static extern int TcSetAttr(int fd, int optionalActions, in Termios termios);

            /// <summary>
            /// Send zero bits on FD
            /// int tcsendbreak(int fd, int duration);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcsendbreak", SetLastError = true)]
            public static extern int TcSendBreak(int fd, int duration);

            /// <summary>
            /// Wait for pending output to be written on FD
            /// int tcdrain(int fd);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcdrain", SetLastError = true)]
            public static extern int TcDrain(int fd);

            /// <summary>
            /// Flush pending data on FD.
            /// Values for QUEUE_SELECTOR(TC{ I,O,IO}FLUSH) are in bits/termios.h
            /// int tcflush(int fd, int queue_selector);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcflush", SetLastError = true)]
            public static extern int TcFlush(int fd, int queueSelector);

            /// <summary>
            /// Suspend or restart transmission on FD.
            /// Values for ACTION(TC[IO]{ OFF,ON}) are in bits/termios.h
            /// int tcflow(int fd, int action);
            /// </summary>
            [DllImport(Libc, EntryPoint = "tcflow", SetLastError = true)]
            public static extern int TcFlow(int fd, int action);

            /// <summary>
            /// Set *TERMIOS_P to indicate raw mode
            /// void cfmakeraw(struct termios *termios_p);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfmakeraw", SetLastError = true)]
            public static extern int CfMakeRaw(ref Termios termios);

            /// <summary>
            /// Return the input baud rate stored in *TERMIOS_P
            /// speed_t cfgetispeed(const struct termios *termios_p);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfgetispeed", SetLastError = true)]
            public static extern uint CfGetISpeed(in Termios termios);

            /// <summary>
            /// Return the output baud rate stored in *TERMIOS_P
            /// speed_t cfgetospeed(const struct termios *termios_p);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfgetospeed", SetLastError = true)]
            public static extern uint CfGetOSpeed(in Termios termios);

            /// <summary>
            /// Set the input baud rate stored in *TERMIOS_P to SPEED
            /// int cfsetispeed(struct termios *termios_p, speed_t speed);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfsetispeed", SetLastError = true)]
            public static extern int CfSetISpeed(in Termios termios, uint speed);

            /// <summary>
            /// Set the output baud rate stored in *TERMIOS_P to SPEED
            /// int cfsetospeed(struct termios *termios_p, speed_t speed);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfsetospeed", SetLastError = true)]
            public static extern int CfSetOSpeed(in Termios termios, uint speed);

            /// <summary>
            /// Set both the input and output baud rates in *TERMIOS_OP to SPEED
            /// int cfsetspeed(struct termios *termios_p, speed_t speed);
            /// </summary>
            [DllImport(Libc, EntryPoint = "cfsetspeed", SetLastError = true)]
            public static extern int CfSetsSpeed(in Termios termios, uint speed);
        }

        internal static partial class Termios_old
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

    }

    /// <summary>
    /// Wraps the Termios communication
    /// </summary>
    internal class TermiosIo
    {
        private int _fd;

        /// <summary>
        /// Put the state of FD into *TERMIOS_P
        /// </summary>
        public Termios TcGetAttr()
        {
            int result = LinuxHelpers.TermiosInterop.TcGetAttr(_fd, out Termios termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }

            return termios;
        }

        /// <summary>
        /// Set the state of FD to *TERMIOS_P.
        /// Values for OPTIONAL_ACTIONS(TCSA*) are in bits/termios.h
        /// </summary>
        public void TcSetAttr(int optionalActions, in Termios termios)
        {
            int result = LinuxHelpers.TermiosInterop.TcSetAttr(_fd, optionalActions, termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Send zero bits on FD
        /// </summary>
        public void TcSendBreak(int duration)
        {
            int result = LinuxHelpers.TermiosInterop.TcSendBreak(_fd, duration);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Wait for pending output to be written on FD
        /// </summary>
        public void TcDrain()
        {
            int result = LinuxHelpers.TermiosInterop.TcDrain(_fd);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Flush pending data on FD.
        /// Values for QUEUE_SELECTOR(TC{ I,O,IO}FLUSH) are in bits/termios.h
        /// </summary>
        public void TcFlush(int queueSelector)
        {
            int result = LinuxHelpers.TermiosInterop.TcFlush(_fd, queueSelector);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Suspend or restart transmission on FD.
        /// Values for ACTION(TC[IO]{ OFF,ON}) are in bits/termios.h
        /// </summary>
        public void TcFlow(int action)
        {
            int result = LinuxHelpers.TermiosInterop.TcFlow(_fd, action);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Set *TERMIOS_P to indicate raw mode
        /// </summary>
        public void CfMakeRaw(ref Termios termios)
        {
            int result = LinuxHelpers.TermiosInterop.CfMakeRaw(ref termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Return the input baud rate stored in *TERMIOS_P
        /// </summary>
        public uint CfGetISpeed(in Termios termios)
        {
            return LinuxHelpers.TermiosInterop.CfGetISpeed(termios);
        }

        /// <summary>
        /// Return the output baud rate stored in *TERMIOS_P
        /// </summary>
        public uint CfGetOSpeed(in Termios termios)
        {
            return LinuxHelpers.TermiosInterop.CfGetOSpeed(termios);
        }

        /// <summary>
        /// Set the input baud rate stored in *TERMIOS_P to SPEED
        /// </summary>
        public void CfSetISpeed(in Termios termios, uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetISpeed(termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Set the output baud rate stored in *TERMIOS_P to SPEED
        /// </summary>
        public void CfSetOSpeed(in Termios termios, uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetOSpeed(termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Set both the input and output baud rates in *TERMIOS_OP to SPEED
        /// </summary>
        public void CfSetsSpeed(in Termios termios, uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetsSpeed(termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

    }
}
