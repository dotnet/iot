// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Device.Ports.SerialPort.Linux.LinuxInterop;

/*
    Termios functions return values:
    - Success: 0
    - Failure: -1 (errno)
    - cfgetispeed() and cfgetospeed(): baud rate

*/

namespace System.Device.Ports.SerialPort.Linux
{
    /// <summary>
    /// Wraps the Termios communication
    /// </summary>
    internal class TermiosIo
    {
        private int _fd;
        private LinuxInterop.Termios _termios;
        private ModemStatus _modemStatus;

        public TermiosIo()
        {
            _fd = 0;
            _modemStatus = new(this);
        }

        public LinuxInterop.Termios Termios => _termios;
        public ModemStatus ModemStatus => _modemStatus;

        private bool IsSetCFlag(uint bit) => (_termios.CFlag & bit) == bit;
        private bool IsSetIFlag(uint bit) => (_termios.IFlag & bit) == bit;
        private bool IsSetLFlag(uint bit) => (_termios.LFlag & bit) == bit;
        private bool IsSetOFlag(uint bit) => (_termios.OFlag & bit) == bit;

        /// <summary>
        /// Load the termios values from the driver
        /// </summary>
        public void TcGetAttr()
        {
            int result = LinuxInterop.TcGetAttr(_fd, out _termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Writes the termios values to the driver
        /// </summary>
        /// <param name="optionalActions">TCSANOW, TCSADRAIN or TCSAFLUSH</param>
        public void TcSetAttr(uint optionalActions)
        {
            int result = LinuxInterop.TcSetAttr(_fd, optionalActions, _termios);
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
            int result = LinuxInterop.TcSendBreak(_fd, duration);
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
            int result = LinuxInterop.TcDrain(_fd);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Discards (clears) data written to the object referred to by fd
        /// but not transmitted, or data received but not read, depending
        /// on the value of queue_selector
        /// TCIFLUSH, TCOFLUSH, TCIOFLUSH
        /// </summary>
        public void TcFlush(uint queueSelector)
        {
            int result = LinuxInterop.TcFlush(_fd, queueSelector);
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
            int result = LinuxInterop.TcFlow(_fd, action);
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
            int result = LinuxInterop.CfMakeRaw(ref _termios);
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
            return LinuxInterop.CfGetISpeed(_termios);
        }

        /// <summary>
        /// Return the output baud rate stored in termios
        /// </summary>
        public uint CfGetOSpeed()
        {
            return LinuxInterop.CfGetOSpeed(_termios);
        }

        /// <summary>
        /// Set the input baud rate stored in termios to SPEED
        /// </summary>
        public void CfSetISpeed(uint speed)
        {
            int result = LinuxInterop.CfSetISpeed(_termios, speed);
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
            int result = LinuxInterop.CfSetOSpeed(_termios, speed);
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
            int result = LinuxInterop.CfSetsSpeed(_termios, speed);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Transfer the Termios structure through ioctl
        /// </summary>
        public void Ioctl(uint command)
        {
            int result = LinuxInterop.Ioctl(_fd, command, ref _termios);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        public uint IoctlRead32(uint command)
        {
            int result = LinuxInterop.IoctlRead32(_fd, command, out uint data);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }

            return data;
        }

        public void IoctlWrite32(uint command, uint data)
        {
            int result = LinuxInterop.IoctlWrite32(_fd, command, data);
            if (LinuxErrors.IsError(result))
            {
                throw new IOException(LinuxErrors.GetLastErrorDescription());
            }
        }

        /// <summary>
        /// Read the register values from the driver
        /// </summary>
        public void Read() => TcGetAttr();

        /// <summary>
        /// Writes the changes to the driver
        /// </summary>
        public void Write() => TcSetAttr(LinuxConstants.TCSANOW);

        public Parity Parity
        {
            get
            {
                var parenb = IsSetCFlag(LinuxConstants.PARENB);
                if (!parenb)
                {
                    return Parity.None;
                }

                if (IsSetCFlag(LinuxConstants.PARODD))
                {
                    return Parity.Odd;
                }

                return Parity.Even;
            }

            set
            {
                _termios.IFlag &= ~LinuxConstants.IGNPAR;
                _termios.IFlag &= ~LinuxConstants.PARMRK;   // parity errors will be marked with a null

                switch (value)
                {
                    case Parity.None:
                        _termios.CFlag &= ~LinuxConstants.PARENB;
                        _termios.IFlag &= ~LinuxConstants.INPCK;
                        break;

                    case Parity.Even:
                        _termios.CFlag |= LinuxConstants.PARENB;
                        _termios.CFlag &= ~LinuxConstants.PARODD;
                        _termios.IFlag |= LinuxConstants.INPCK;
                        break;

                    case Parity.Odd:
                        _termios.CFlag |= LinuxConstants.PARENB;
                        _termios.CFlag |= LinuxConstants.PARODD;
                        _termios.IFlag |= LinuxConstants.INPCK;
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
            get => IsSetCFlag(LinuxConstants.CSTOPB) ? 2 : 1;
            set
            {
                if (value == 1)
                {
                    _termios.CFlag &= ~LinuxConstants.CSTOPB;
                }
                else if (value == 2)
                {
                    _termios.CFlag |= LinuxConstants.CSTOPB;
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
                if (IsSetCFlag(LinuxConstants.CS5))
                {
                    return 5;
                }

                if (IsSetCFlag(LinuxConstants.CS6))
                {
                    return 6;
                }

                if (IsSetCFlag(LinuxConstants.CS7))
                {
                    return 7;
                }

                if (IsSetCFlag(LinuxConstants.CS8))
                {
                    return 8;
                }

                return 0;
            }

            set
            {
                _termios.CFlag &= ~LinuxConstants.CSIZE;
                switch (value)
                {
                    case 5:
                        _termios.CFlag |= LinuxConstants.CS5;
                        break;

                    case 6:
                        _termios.CFlag |= LinuxConstants.CS6;
                        break;

                    case 7:
                        _termios.CFlag |= LinuxConstants.CS7;
                        break;

                    case 8:
                        _termios.CFlag |= LinuxConstants.CS8;
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
                var isRts = IsSetCFlag(LinuxConstants.CRTSCTS);
                var isXonXoff = IsSetCFlag(LinuxConstants.IXON);

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
                _termios.CFlag &= ~LinuxConstants.CRTSCTS;
                _termios.IFlag &= ~(LinuxConstants.IXON | LinuxConstants.IXOFF | LinuxConstants.IXANY);
                if (value == Handshake.None)
                {
                    return;
                }

                if (value == Handshake.RequestToSend)
                {
                    _termios.CFlag |= LinuxConstants.CRTSCTS;
                    return;
                }

                if (value == Handshake.XOnXOff)
                {
                    _termios.CFlag |= LinuxConstants.IXON | LinuxConstants.IXOFF | LinuxConstants.IXANY;
                    return;
                }

                if (value == Handshake.RequestToSendXOnXOff)
                {
                    _termios.CFlag |= LinuxConstants.CRTSCTS;
                    _termios.CFlag |= LinuxConstants.IXON | LinuxConstants.IXOFF | LinuxConstants.IXANY;
                    return;
                }
            }
        }

        public bool ReadDataFlag
        {
            get => IsSetCFlag(LinuxConstants.CREAD);
            set => _termios.CFlag |= LinuxConstants.CREAD;
        }

        public bool CLocalFlag
        {
            get => IsSetCFlag(LinuxConstants.CLOCAL);
            set
            {
                if (value)
                {
                    _termios.CFlag |= LinuxConstants.CLOCAL;
                }
                else
                {
                    _termios.CFlag &= ~LinuxConstants.CLOCAL;
                }
            }
        }

        public bool CanonicalMode
        {
            get => IsSetLFlag(LinuxConstants.ICANON);
            set
            {
                if (value)
                {
                    _termios.LFlag |= LinuxConstants.ICANON;
                }
                else
                {
                    _termios.LFlag &= ~LinuxConstants.ICANON;
                }
            }
        }

        public bool Echo
        {
            get => IsSetLFlag(LinuxConstants.ECHO);
            set
            {
                if (value)
                {
                    _termios.LFlag |= LinuxConstants.ECHO;
                    _termios.LFlag |= LinuxConstants.ECHOE;
                    _termios.LFlag |= LinuxConstants.ECHONL;
                }
                else
                {
                    _termios.LFlag &= ~LinuxConstants.ECHO;
                    _termios.LFlag &= ~LinuxConstants.ECHOE;
                    _termios.LFlag &= ~LinuxConstants.ECHONL;
                }
            }
        }

        public bool InputSignalChars
        {
            get => IsSetLFlag(LinuxConstants.ISIG);
            set
            {
                if (value)
                {
                    _termios.LFlag |= LinuxConstants.ISIG;
                }
                else
                {
                    _termios.LFlag &= ~LinuxConstants.ISIG;
                }
            }
        }

        public bool InputIgnoreBreak
        {
            get => IsSetIFlag(LinuxConstants.IGNBRK);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.IGNBRK;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.IGNBRK;
                }
            }
        }

        public bool InputSignalInterruptOnBreak
        {
            get => IsSetIFlag(LinuxConstants.BRKINT);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.BRKINT;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.BRKINT;
                }
            }
        }

        public bool InputMarkParityAndFramingErrors
        {
            get => IsSetIFlag(LinuxConstants.PARMRK);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.PARMRK;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.PARMRK;
                }
            }
        }

        public bool InputStripEightBit
        {
            get => IsSetIFlag(LinuxConstants.ISTRIP);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.ISTRIP;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.ISTRIP;
                }
            }
        }

        public bool InputMapNewLineToCarriageReturn
        {
            get => IsSetIFlag(LinuxConstants.INLCR);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.INLCR;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.INLCR;
                }
            }
        }

        public bool InputIgnoreCarriageReturn
        {
            get => IsSetIFlag(LinuxConstants.IGNCR);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.IGNCR;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.IGNCR;
                }
            }
        }

        public bool InputMapCarriageReturnToNewLine
        {
            get => IsSetIFlag(LinuxConstants.ICRNL);
            set
            {
                if (value)
                {
                    _termios.IFlag |= LinuxConstants.ICRNL;
                }
                else
                {
                    _termios.IFlag &= ~LinuxConstants.ICRNL;
                }
            }
        }

        public bool OutputInterpretOutputBytes
        {
            get => IsSetOFlag(LinuxConstants.OPOST);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.OPOST;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.OPOST;
                }
            }
        }

        public bool OutputMapCarriageReturnToNewLine
        {
            get => IsSetOFlag(LinuxConstants.OCRNL);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.OCRNL;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.OCRNL;
                }
            }
        }

        public bool OutputDontOutputCROnColumn0
        {
            get => IsSetOFlag(LinuxConstants.ONOCR);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.ONOCR;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.ONOCR;
                }
            }
        }

        public bool OutputDontOutputCR
        {
            get => IsSetOFlag(LinuxConstants.ONLRET);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.ONLRET;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.ONLRET;
                }
            }
        }

        public bool OutputSendFillCharactersToDelay
        {
            get => IsSetOFlag(LinuxConstants.OFILL);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.OFILL;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.OFILL;
                }
            }
        }

        public bool OutputMapNewLineToCarriageReturnAndNewLine
        {
            get => IsSetOFlag(LinuxConstants.ONLCR);
            set
            {
                if (value)
                {
                    _termios.OFlag |= LinuxConstants.ONLCR;
                }
                else
                {
                    _termios.OFlag &= ~LinuxConstants.ONLCR;
                }
            }
        }

        /// <summary>
        /// Expressed in deciseconds
        /// </summary>
        public byte VMin
        {
            get => _termios.Cc[LinuxConstants.VMIN];
            set => _termios.Cc[LinuxConstants.VMIN] = value;
        }

        /// <summary>
        /// Expressed in deciseconds
        /// </summary>
        public byte VTime
        {
            get => _termios.Cc[LinuxConstants.VTIME];
            set => _termios.Cc[LinuxConstants.VTIME] = value;
        }

        public uint GetStandardSpeed(int baudRate)
            => baudRate switch
            {
                0 => LinuxConstants.B0,
                50 => LinuxConstants.B50,
                75 => LinuxConstants.B75,
                110 => LinuxConstants.B110,
                134 => LinuxConstants.B134,
                150 => LinuxConstants.B150,
                200 => LinuxConstants.B200,
                300 => LinuxConstants.B300,
                600 => LinuxConstants.B600,
                1200 => LinuxConstants.B1200,
                1800 => LinuxConstants.B1800,
                2400 => LinuxConstants.B2400,
                4800 => LinuxConstants.B4800,
                9600 => LinuxConstants.B9600,
                19200 => LinuxConstants.B19200,
                38400 => LinuxConstants.B38400,
                _ => 0,
            };

        public uint CustomBaudRate
        {
            get
            {
                if (IsSetCFlag(LinuxConstants.CBAUDEX))
                {
                    return _termios.OSpeed;
                }

                return 0;
            }

            set
            {
                // A similar approach: https://github.com/dotnet/runtime/issues/64507
                // ===
                _termios.CFlag &= ~LinuxConstants.CBAUD;
                _termios.CFlag |= LinuxConstants.CBAUDEX;
                // _termios.CFlag |= LinuxConstants.BOTHER; // same value
                _termios.ISpeed = value;
                _termios.OSpeed = value;
                Ioctl(LinuxConstants.TCSETS2);
            }
        }

        public uint AvailableBytes => IoctlRead32(LinuxConstants.FIONREAD);
        public uint OutputBufferBytes => IoctlRead32(LinuxConstants.TIOCOUTQ);
    }
}
