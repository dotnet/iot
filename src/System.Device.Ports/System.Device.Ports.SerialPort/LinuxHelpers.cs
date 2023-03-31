// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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

    public const int NCCS 19
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
        public uint ISpeed; // this is part of the termios2
        public uint OSpeed; // this is part of the termios2
    }

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
        //public CapabilityFlags Capabilities;
    }

    internal static class TermiosFlags
    {
        /* c_cc characters */
        public const uint VINTR = 0;
        public const uint VQUIT = 1;
        public const uint VERASE = 2;
        public const uint VKILL = 3;
        public const uint VEOF = 4;
        public const uint VTIME = 5;
        public const uint VMIN = 6;
        public const uint VSWTC = 7;
        public const uint VSTART = 8;
        public const uint VSTOP = 9;
        public const uint VSUSP = 10;
        public const uint VEOL = 11;
        public const uint VREPRINT = 12;
        public const uint VDISCARD = 13;
        public const uint VWERASE = 14;
        public const uint VLNEXT = 15;
        public const uint VEOL2 = 16;

        /* c_iflag bits */
        public const uint IUCLC = 0x0200;
        public const uint IXON = 0x0400;
        public const uint IXOFF = 0x1000;
        public const uint IMAXBEL = 0x2000;
        public const uint IUTF8 = 0x4000;

        /* c_oflag bits */

        /// <summary>
        /// (not in POSIX) Map lowercase characters to uppercase on output
        /// </summary>
        public const uint OLCUC = 0x00002;

        /// <summary>
        /// (XSI) Map NL to CR-NL on output
        /// </summary>
        public const uint ONLCR = 0x00004;
        public const uint NLDLY = 0x00100;
        public const uint NL0 = 0x00000;
        public const uint NL1 = 0x00100;
        public const uint CRDLY = 0x00600;
        public const uint CR0 = 0x00000;
        public const uint CR1 = 0x00200;
        public const uint CR2 = 0x00400;
        public const uint CR3 = 0x00600;
        public const uint TABDLY = 0x01800;
        public const uint TAB0 = 0x00000;
        public const uint TAB1 = 0x00800;
        public const uint TAB2 = 0x01000;
        public const uint TAB3 = 0x01800;
        public const uint XTABS = 0x01800;
        public const uint BSDLY = 0x02000;
        public const uint BS0 = 0x00000;
        public const uint BS1 = 0x02000;
        public const uint VTDLY = 0x04000;
        public const uint VT0 = 0x00000;
        public const uint VT1 = 0x04000;
        public const uint FFDLY = 0x08000;
        public const uint FF0 = 0x00000;
        public const uint FF1 = 0x08000;

        /* c_cflag bit meaning */
        public const uint CBAUD = 0x0000100f;
        public const uint CSIZE = 0x00000030;
        public const uint CS5 = 0x00000000;
        public const uint CS6 = 0x00000010;
        public const uint CS7 = 0x00000020;
        public const uint CS8 = 0x00000030;
        public const uint CSTOPB = 0x00000040;
        public const uint CREAD = 0x00000080;
        public const uint PARENB = 0x00000100;
        public const uint PARODD = 0x00000200;
        public const uint HUPCL = 0x00000400;
        public const uint CLOCAL = 0x00000800;
        public const uint CBAUDEX = 0x00001000;
        public const uint BOTHER = 0x00001000;
        public const uint B57600 = 0x00001001;
        public const uint B115200 = 0x00001002;
        public const uint B230400 = 0x00001003;
        public const uint B460800 = 0x00001004;
        public const uint B500000 = 0x00001005;
        public const uint B576000 = 0x00001006;
        public const uint B921600 = 0x00001007;
        public const uint B1000000 = 0x00001008;
        public const uint B1152000 = 0x00001009;
        public const uint B1500000 = 0x0000100a;
        public const uint B2000000 = 0x0000100b;
        public const uint B2500000 = 0x0000100c;
        public const uint B3000000 = 0x0000100d;
        public const uint B3500000 = 0x0000100e;
        public const uint B4000000 = 0x0000100f;
        public const uint CIBAUD = 0x100f0000;   /* input baud rate */

        /* c_lflag bits */
        public const uint ISIG = 0x00001;
        public const uint ICANON = 0x00002;
        public const uint XCASE = 0x00004;
        public const uint ECHO = 0x00008;
        public const uint ECHOE = 0x00010;
        public const uint ECHOK = 0x00020;
        public const uint ECHONL = 0x00040;
        public const uint NOFLSH = 0x00080;
        public const uint TOSTOP = 0x00100;
        public const uint ECHOCTL = 0x00200;
        public const uint ECHOPRT = 0x00400;
        public const uint ECHOKE = 0x00800;
        public const uint FLUSHO = 0x01000;
        public const uint PENDIN = 0x04000;
        public const uint IEXTEN = 0x08000;
        public const uint EXTPROC = 0x10000;

        /* tcsetattr uses these */
        public const uint TCSANOW = 0;
        public const uint TCSADRAIN = 1;
        public const uint TCSAFLUSH = 2;

        /* c_iflag bits */
        public const uint IGNBRK = 0x001;           /* Ignore break condition */
        public const uint BRKINT = 0x002;           /* Signal interrupt on break */
        public const uint IGNPAR = 0x004;           /* Ignore characters with parity errors */
        public const uint PARMRK = 0x008;           /* Mark parity and framing errors */
        public const uint INPCK = 0x010;            /* Enable input parity check */
        public const uint ISTRIP = 0x020;           /* Strip 8th bit off characters */
        public const uint INLCR = 0x040;            /* Map NL to CR on input */
        public const uint IGNCR = 0x080;            /* Ignore CR */
        public const uint ICRNL = 0x100;            /* Map CR to NL on input */
        public const uint IXANY = 0x800;            /* Any character will restart after stop */

        /* c_oflag bits */

        /// <summary>
        /// Perform output processing
        /// </summary>
        public const uint OPOST = 0x01;

        /// <summary>
        /// Map CR to NL on output
        /// </summary>
        public const uint OCRNL = 0x08;

        /// <summary>
        /// Don't output CR at column 0
        /// </summary>
        public const uint ONOCR = 0x10;

        /// <summary>
        /// Don't output CR
        /// </summary>
        public const uint ONLRET = 0x20;

        /// <summary>
        /// Send fill characters for a delay, rather than using a timed delay
        /// </summary>
        public const uint OFILL = 0x40;

        /// <summary>
        /// Fill character is ASCII DEL (0177).
        /// If unset, fill character is ASCII NUL ('\0')
        /// Not available on Linux
        /// </summary>
        public const uint OFDEL = 0x80;

        /* c_cflag bit meaning */
        /* Common CBAUD rates */
        public const uint B0 = 0x00000000;      /* hang up */
        public const uint B50 = 0x00000001;
        public const uint B75 = 0x00000002;
        public const uint B110 = 0x00000003;
        public const uint B134 = 0x00000004;
        public const uint B150 = 0x00000005;
        public const uint B200 = 0x00000006;
        public const uint B300 = 0x00000007;
        public const uint B600 = 0x00000008;
        public const uint B1200 = 0x00000009;
        public const uint B1800 = 0x0000000a;
        public const uint B2400 = 0x0000000b;
        public const uint B4800 = 0x0000000c;
        public const uint B9600 = 0x0000000d;
        public const uint B19200 = 0x0000000e;
        public const uint B38400 = 0x0000000f;
        public const uint EXTA = B19200;
        public const uint EXTB = B38400;

        public const uint ADDRB = 0x20000000;       /* address bit */
        public const uint CMSPAR = 0x40000000;      /* mark or space (stick) parity */
        public const uint CRTSCTS = 0x80000000;     /* flow control */

        public const uint IBSHIFT = 16;             /* Shift from CBAUD to CIBAUD */

        /* tcflow() ACTION argument and TCXONC use these */
        public const uint TCOOFF = 0;               /* Suspend output */
        public const uint TCOON = 1;                /* Restart suspended output */
        public const uint TCIOFF = 2;               /* Send a STOP character */
        public const uint TCION = 3;                /* Send a START character */

        /* tcflush() QUEUE_SELECTOR argument and TCFLSH use these */
        public const uint TCIFLUSH = 0;             /* Discard data received but not yet read */
        public const uint TCOFLUSH = 1;             /* Discard data written but not yet sent */
        public const uint TCIOFLUSH = 2;            /* Discard all pending data */
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
        private Termios _termios;

        public Termios Termios => _termios;

        private bool IsSetCFlag(uint bit) => (_termios.CFlag & bit) == bit;
        private bool IsSetIFlag(uint bit) => (_termios.IFlag & bit) == bit;
        private bool IsSetLFlag(uint bit) => (_termios.LFlag & bit) == bit;
        private bool IsSetOFlag(uint bit) => (_termios.OFlag & bit) == bit;

        /// <summary>
        /// Load the termios values from the driver
        /// </summary>
        public void TcGetAttr()
        {
            int result = LinuxHelpers.TermiosInterop.TcGetAttr(_fd, out _termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Writes the termios values to the driver
        /// </summary>
        public void TcSetAttr(int optionalActions)
        {
            int result = LinuxHelpers.TermiosInterop.TcSetAttr(_fd, optionalActions, _termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Send zero bits on the driver (ms)
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
        /// Wait for pending output to be written on the driver
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
        /// Flush pending data on the driver.
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
        /// Suspend or restart transmission on the driver.
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
        /// Set termios to indicate raw mode
        /// </summary>
        public void CfMakeRaw()
        {
            int result = LinuxHelpers.TermiosInterop.CfMakeRaw(ref _termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Return the input baud rate stored in termios
        /// </summary>
        public uint CfGetISpeed()
        {
            return LinuxHelpers.TermiosInterop.CfGetISpeed(_termios);
        }

        /// <summary>
        /// Return the output baud rate stored in termios
        /// </summary>
        public uint CfGetOSpeed()
        {
            return LinuxHelpers.TermiosInterop.CfGetOSpeed(_termios);
        }

        /// <summary>
        /// Set the input baud rate stored in termios to SPEED
        /// </summary>
        public void CfSetISpeed(uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetISpeed(_termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Set the output baud rate stored in termios to SPEED
        /// </summary>
        public void CfSetOSpeed(uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetOSpeed(_termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Set both the input and output baud rates in termios to SPEED
        /// </summary>
        public void CfSetsSpeed(uint speed)
        {
            int result = LinuxHelpers.TermiosInterop.CfSetsSpeed(_termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        public Parity Parity
        {
            get
            {
                var parenb = IsSetCFlag(TermiosFlags.PARENB);
                if (!parenb)
                {
                    return Parity.None;
                }

                if (IsSetCFlag(TermiosFlags.PARODD))
                {
                    return Parity.Odd;
                }

                return Parity.Even;
            }

            set
            {
                switch (value)
                {
                    case Parity.None:
                        _termios.CFlag &= ~TermiosFlags.PARENB;
                        break;

                    case Parity.Even:
                        _termios.CFlag |= TermiosFlags.PARENB;
                        _termios.CFlag &= ~TermiosFlags.PARODD;
                        break;

                    case Parity.Odd:
                        _termios.CFlag |= TermiosFlags.PARENB;
                        _termios.CFlag |= TermiosFlags.PARODD;
                        break;

                    case Parity.Mark:
                    case Parity.Space:
                    // https://viereck.ch/linux-mark-space-parity/
                    default:
                        throw new NotSupportedException($"Parity {value} is not supported on this platform");

                }
            }
        }

        public int StopBits
        {
            get => IsSetCFlag(TermiosFlags.CSTOPB) ? 2 : 1;
            set
            {
                if (value == 1)
                {
                    _termios.CFlag &= ~TermiosFlags.CSTOPB;
                }
                else if (value == 2)
                {
                    _termios.CFlag |= TermiosFlags.CSTOPB;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Stop bits {value} is not supported on this platform");
                }
            }
        }

        public int DataBits
        {
            get
            {
                if (IsSetCFlag(TermiosFlags.CS5))
                {
                    return 5;
                }

                if (IsSetCFlag(TermiosFlags.CS6))
                {
                    return 6;
                }

                if (IsSetCFlag(TermiosFlags.CS7))
                {
                    return 7;
                }

                if (IsSetCFlag(TermiosFlags.CS8))
                {
                    return 8;
                }

                return 0;
            }

            set
            {
                _termios.CFlag &= ~TermiosFlags.CSIZE;
                switch (value)
                {
                    case 5:
                        _termios.CFlag |= TermiosFlags.CS5;
                        break;

                    case 6:
                        _termios.CFlag |= TermiosFlags.CS6;
                        break;

                    case 7:
                        _termios.CFlag |= TermiosFlags.CS7;
                        break;

                    case 8:
                        _termios.CFlag |= TermiosFlags.CS8;
                        break;

                    default:
                        throw new NotSupportedException($"Data bits {value} is not supported on this platform");
                }
            }
        }

        public Handshake Handshake
        {
            get
            {
                // we can avoid testing xon, xoff and xany as
                // the flags are private and we set all the three
                var isRts = IsSetCFlag(TermiosFlags.CRTSCTS);
                var isXonXoff = IsSetCFlag(TermiosFlags.IXON);

                if (isRts)
                {
                    if (isXonXoff)
                    {
                        return Handshake.RequestToSendXOnXOff;
                    }

                    return Handshake.RequestToSend;
                }
                else
                {
                    if (isXonXoff)
                    {
                        return Handshake.XOnXOff;
                    }

                    return Handshake.None;
                }
            }

            set
            {
                _termios.CFlag &= ~TermiosFlags.CRTSCTS;
                _termios.IFlag &= ~(TermiosFlags.IXON | TermiosFlags.IXOFF | TermiosFlags.IXANY);
                if (value == Handshake.None)
                {
                    return;
                }

                if (value == Handshake.RequestToSend)
                {
                    _termios.CFlag |= TermiosFlags.CRTSCTS;
                    return;
                }

                if (value == Handshake.XOnXOff)
                {
                    _termios.CFlag |= TermiosFlags.IXON | TermiosFlags.IXOFF | TermiosFlags.IXANY;
                    return;
                }

                if (value == Handshake.RequestToSendXOnXOff)
                {
                    _termios.CFlag |= TermiosFlags.CRTSCTS;
                    _termios.CFlag |= TermiosFlags.IXON | TermiosFlags.IXOFF | TermiosFlags.IXANY;
                    return;
                }
            }
        }

        public bool ReadDataFlag
        {
            get => IsSetCFlag(TermiosFlags.CREAD);
            set => _termios.CFlag |= TermiosFlags.CREAD;
        }

        public bool CLocalFlag
        {
            get => IsSetCFlag(TermiosFlags.CLOCAL);
            set
            {
                if (value)
                {
                    _termios.CFlag |= TermiosFlags.CLOCAL;
                }
                else
                {
                    _termios.CFlag &= ~TermiosFlags.CLOCAL;
                }
            }
        }

        public bool CanonicalMode
        {
            get => IsSetLFlag(TermiosFlags.ICANON);
            set
            {
                if (value)
                {
                    _termios.LFlag |= TermiosFlags.ICANON;
                }
                else
                {
                    _termios.LFlag &= ~TermiosFlags.ICANON;
                }
            }
        }

        public bool Echo
        {
            get => IsSetLFlag(TermiosFlags.ECHO);
            set
            {
                if (value)
                {
                    _termios.LFlag |= TermiosFlags.ECHO;
                    _termios.LFlag |= TermiosFlags.ECHOE;
                    _termios.LFlag |= TermiosFlags.ECHONL;
                }
                else
                {
                    _termios.LFlag &= ~TermiosFlags.ECHO;
                    _termios.LFlag &= ~TermiosFlags.ECHOE;
                    _termios.LFlag &= ~TermiosFlags.ECHONL;
                }
            }
        }

        public bool InputSignalChars
        {
            get => IsSetLFlag(TermiosFlags.ISIG);
            set
            {
                if (value)
                {
                    _termios.LFlag |= TermiosFlags.ISIG;
                }
                else
                {
                    _termios.LFlag &= ~TermiosFlags.ISIG;
                }
            }
        }

        public bool InputIgnoreBreak
        {
            get => IsSetIFlag(TermiosFlags.IGNBRK);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.IGNBRK;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.IGNBRK;
                }
            }
        }

        public bool InputSignalInterruptOnBreak
        {
            get => IsSetIFlag(TermiosFlags.BRKINT);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.BRKINT;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.BRKINT;
                }
            }
        }

        public bool InputMarkParityAndFramingErrors
        {
            get => IsSetIFlag(TermiosFlags.PARMRK);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.PARMRK;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.PARMRK;
                }
            }
        }

        public bool InputStripEightBit
        {
            get => IsSetIFlag(TermiosFlags.ISTRIP);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.ISTRIP;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.ISTRIP;
                }
            }
        }

        public bool InputMapNewLineToCarriageReturn
        {
            get => IsSetIFlag(TermiosFlags.INLCR);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.INLCR;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.INLCR;
                }
            }
        }

        public bool InputIgnoreCarriageReturn
        {
            get => IsSetIFlag(TermiosFlags.IGNCR);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.IGNCR;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.IGNCR;
                }
            }
        }

        public bool InputMapCarriageReturnToNewLine
        {
            get => IsSetIFlag(TermiosFlags.ICRNL);
            set
            {
                if (value)
                {
                    _termios.IFlag |= TermiosFlags.ICRNL;
                }
                else
                {
                    _termios.IFlag &= ~TermiosFlags.ICRNL;
                }
            }
        }

        public bool OutputInterpretOutputBytes
        {
            get => IsSetOFlag(TermiosFlags.OPOST);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.OPOST;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.OPOST;
                }
            }
        }

        public bool OutputMapCarriageReturnToNewLine
        {
            get => IsSetOFlag(TermiosFlags.OCRNL);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.OCRNL;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.OCRNL;
                }
            }
        }

        public bool OutputDontOutputCROnColumn0
        {
            get => IsSetOFlag(TermiosFlags.ONOCR);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.ONOCR;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.ONOCR;
                }
            }
        }

        public bool OutputDontOutputCR
        {
            get => IsSetOFlag(TermiosFlags.ONLRET);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.ONLRET;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.ONLRET;
                }
            }
        }

        public bool OutputSendFillCharactersToDelay
        {
            get => IsSetOFlag(TermiosFlags.OFILL);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.OFILL;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.OFILL;
                }
            }
        }

        public bool OutputMapNewLineToCarriageReturnAndNewLine
        {
            get => IsSetOFlag(TermiosFlags.ONLCR);
            set
            {
                if (value)
                {
                    _termios.OFlag |= TermiosFlags.ONLCR;
                }
                else
                {
                    _termios.OFlag &= ~TermiosFlags.ONLCR;
                }
            }
        }

        /// <summary>
        /// Expressed in deciseconds
        /// </summary>
        public byte VMin
        {
            get => _termios.Cc[TermiosFlags.VMIN];
            set => _termios.Cc[TermiosFlags.VMIN] = value;
        }

        /// <summary>
        /// Expressed in deciseconds
        /// </summary>
        public byte VTime
        {
            get => _termios.Cc[TermiosFlags.VTIME];
            set => _termios.Cc[TermiosFlags.VTIME] = value;
        }

        public uint GetStandardSpeed(int baudRate)
            => baudRate switch
            {
                0 => TermiosFlags.B0,
                50 => TermiosFlags.B50,
                75 => TermiosFlags.B75,
                110 => TermiosFlags.B110,
                134 => TermiosFlags.B134,
                150 => TermiosFlags.B150,
                200 => TermiosFlags.B200,
                300 => TermiosFlags.B300,
                600 => TermiosFlags.B600,
                1200 => TermiosFlags.B1200,
                1800 => TermiosFlags.B1800,
                2400 => TermiosFlags.B2400,
                4800 => TermiosFlags.B4800,
                9600 => TermiosFlags.B9600,
                19200 => TermiosFlags.B19200,
                38400 => TermiosFlags.B38400,
                _ => 0,
            };

    }
}
