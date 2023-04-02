// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Termios man page: https://linux.die.net/man/3/termios
    Code browser: https://codebrowser.dev/glibc/glibc/termios/termios.h.html
    Ioctl.h: https://github.com/torvalds/linux/blob/7b50567bdcad8925ca1e075feb7171c12015afd1/include/uapi/asm-generic/ioctl.h

    *** Calculation of the IOCTL v2 values ***
    #define _IOC_NONE  0U
    #define _IOC_WRITE 1U
    #define _IOC_READ  2U

    #define _IOC_NRBITS   8
    #define _IOC_TYPEBITS 8
    #define _IOC_SIZEBITS 14
    #define _IOC_DIRBITS  2

    #define _IOC_NRMASK     ((1 << _IOC_NRBITS)-1)      // (1 << 8)  - 1 = 255
    #define _IOC_TYPEMASK   ((1 << _IOC_TYPEBITS)-1)    // (1 << 8)  - 1 = 255
    #define _IOC_SIZEMASK   ((1 << _IOC_SIZEBITS)-1)    // (1 << 14) - 1 = 16383
    #define _IOC_DIRMASK    ((1 << _IOC_DIRBITS)-1)     // (1 << 2)  - 1 = 3

    #define _IOC_NRSHIFT    0
    #define _IOC_TYPESHIFT  (_IOC_NRSHIFT+_IOC_NRBITS)        // 0  + 8  == 8
    #define _IOC_SIZESHIFT  (_IOC_TYPESHIFT+_IOC_TYPEBITS)    // 8  + 8  == 16
    #define _IOC_DIRSHIFT   (_IOC_SIZESHIFT+_IOC_SIZEBITS)    // 16 + 14 == 30

    #define _IOR(type,nr,size) _IOC(_IOC_READ,(type),(nr),(_IOC_TYPECHECK(size)))
    #define _IOW(type,nr,size) _IOC(_IOC_WRITE,(type),(nr),(_IOC_TYPECHECK(size)))
    #define _IOC(dir,type,nr,size) \
        (((dir)  << _IOC_DIRSHIFT) | \
        ((type) << _IOC_TYPESHIFT) | \
        ((nr)   << _IOC_NRSHIFT) | \
        ((size) << _IOC_SIZESHIFT))

    #define TCGETS2  _IOR('T', 0x2A, struct termios2)
    #define TCSETS2  _IOW('T', 0x2B, struct termios2)
    #define TCSETSW2 _IOW('T', 0x2C, struct termios2)
    #define TCSETSF2 _IOW('T', 0x2D, struct termios2)

    'T' == 0x54
    sizeof(struct termios2) == 4*int + 1 + 19 + 2*int == 44 == 0x2C

    TCGETS2  == (_IOC_READ << _IOC_DIRSHIFT) | ('T' << _IOC_TYPESHIFT) |
                 (0x2A << _IOC_NRSHIFT) | (sizeof(struct termios2) << _IOC_SIZESHIFT)
             == (2 << 30)   | (0x54 << 8) | (0x2A << 0) | (0x2C << 16)
             == 0x8000_0000 | 0x0000_5400 | 0x0000_002A | 0x002C_0000  == 0x802C542A

    TCSETS2  == (1 << 30)   | (0x54 << 8) | (0x2B << 0) | (0x2C << 16) == 0x402C542B
    TCSETSW2 == (1 << 30)   | (0x54 << 8) | (0x2C << 0) | (0x2C << 16) == 0x402C542C
    TCSETSF2 == (1 << 30)   | (0x54 << 8) | (0x2C << 0) | (0x2C << 16) == 0x402C542D
*/

namespace System.Device.Ports.SerialPort.Linux
{
    internal static class LinuxConstants
    {
        public const string Libc = "libc";
        public const int NCCS = 19;

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

        /* IOCTLS related */

        public const uint TIOCM_LE = 0x001;
        public const uint TIOCM_DTR = 0x002;
        public const uint TIOCM_RTS = 0x004;
        public const uint TIOCM_ST = 0x008;
        public const uint TIOCM_SR = 0x010;
        public const uint TIOCM_CTS = 0x020;
        public const uint TIOCM_CAR = 0x040;
        public const uint TIOCM_RNG = 0x080;
        public const uint TIOCM_DSR = 0x100;
        public const uint TIOCM_CD = TIOCM_CAR;
        public const uint TIOCM_RI = TIOCM_RNG;
        public const uint TIOCM_OUT1 = 0x2000;
        public const uint TIOCM_OUT2 = 0x4000;
        public const uint TIOCM_LOOP = 0x8000;

        /* IOCTLs */

        /// <summary>
        /// Gets the current serial port settings
        /// </summary>
        public const uint TCGETS = 0x5401;

        /// <summary>
        /// Sets the serial port settings immediately
        /// </summary>
        public const uint TCSETS = 0x5402;

        /// <summary>
        /// Sets the serial port settings after allowing
        /// the input and output buffers to drain/empty
        /// </summary>
        public const uint TCSETSW = 0x5403;

        /// <summary>
        /// Sets the serial port settings after flushing
        /// the input and output buffers
        /// </summary>
        public const uint TCSETSF = 0x5404;
        public const uint TCGETA = 0x5405;
        public const uint TCSETA = 0x5406;
        public const uint TCSETAW = 0x5407;
        public const uint TCSETAF = 0x5408;

        /// <summary>
        /// Sends a break for the given time
        /// </summary>
        public const uint TCSBRK = 0x5409;

        /// <summary>
        /// Controls software flow control
        /// </summary>
        public const uint TCXONC = 0x540A;

        /// <summary>
        /// Flushes the input and/or output queue
        /// </summary>
        public const uint TCFLSH = 0x540B;
        public const uint TIOCEXCL = 0x540C;
        public const uint TIOCNXCL = 0x540D;
        public const uint TIOCSCTTY = 0x540E;
        public const uint TIOCGPGRP = 0x540F;
        public const uint TIOCSPGRP = 0x5410;

        /// <summary>
        /// Get the number of bytes in the output buffer
        /// https://man7.org/linux/man-pages/man2/ioctl_tty.2.html
        /// </summary>
        public const uint TIOCOUTQ = 0x5411;
        public const uint TIOCSTI = 0x5412;
        public const uint TIOCGWINSZ = 0x5413;
        public const uint TIOCSWINSZ = 0x5414;

        /// <summary>
        /// Returns the state of the "MODEM" bits
        /// </summary>
        public const uint TIOCMGET = 0x5415;
        public const uint TIOCMBIS = 0x5416;
        public const uint TIOCMBIC = 0x5417;

        /// <summary>
        /// Sets the state of the "MODEM" bits
        /// </summary>
        public const uint TIOCMSET = 0x5418;
        public const uint TIOCGSOFTCAR = 0x5419;
        public const uint TIOCSSOFTCAR = 0x541A;

        /// <summary>
        /// Returns the number of bytes in the input buffer
        /// </summary>
        public const uint FIONREAD = 0x541B;
        public const uint TIOCINQ = FIONREAD;
        public const uint TIOCLINUX = 0x541C;
        public const uint TIOCCONS = 0x541D;
        public const uint TIOCGSERIAL = 0x541E;
        public const uint TIOCSSERIAL = 0x541F;
        public const uint TIOCPKT = 0x5420;
        public const uint FIONBIO = 0x5421;
        public const uint TIOCNOTTY = 0x5422;
        public const uint TIOCSETD = 0x5423;
        public const uint TIOCGETD = 0x5424;

        /// <summary>
        /// Needed for POSIX tcsendbreak()
        /// </summary>
        public const uint TCSBRKP = 0x5425;

        /// <summary>
        /// BSD compatibility
        /// </summary>
        public const uint TIOCSBRK = 0x5427;

        /// <summary>
        /// BSD compatibility
        /// </summary>
        public const uint TIOCCBRK = 0x5428;

        /// <summary>
        /// Return the session ID of FD
        /// </summary>
        public const uint TIOCGSID = 0x5429;
        public const uint TIOCGRS485 = 0x542E;

        public const uint TCGETS2 = 0x802C542A;
        public const uint TCSETS2 = 0x402C542B;
        public const uint TCSETSW2 = 0x402C542C;
        public const uint TCSETSF2 = 0x402C542D;
    }
}
