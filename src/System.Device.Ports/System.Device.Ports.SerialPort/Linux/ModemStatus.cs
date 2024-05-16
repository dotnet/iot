// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
https://www.thegeekdiary.com/serial-port-programming-reading-writing-status-of-control-lines-dtr-rts-cts-dsr/
https://www.linuxjournal.com/article/6226
*/

namespace System.Device.Ports.SerialPort.Linux
{
    /// <summary>
    /// Status of the serial port lines
    /// </summary>
    public class ModemStatus
    {
        private readonly TermiosIo _termiosIo;

        /// <summary>
        /// The raw value of the flags representing the status
        /// </summary>
        public uint Status { get; private set; }

        /// <summary>
        /// Creates an instance of the ModemStatus given the Termios wrapper
        /// </summary>
        internal ModemStatus(TermiosIo termiosIo)
        {
            _termiosIo = termiosIo;
            Read();
        }

        /// <summary>
        /// Reads the status from the driver
        /// </summary>
        public void Read()
            => Status = _termiosIo.IoctlRead32(LinuxConstants.TIOCMGET);

        /// <summary>
        /// Writes the status to the driver
        /// </summary>
        public void Write()
            => _termiosIo.IoctlWrite32(LinuxConstants.TIOCMSET, Status);

        private uint GetFlag(uint flag) => Status & flag;
        private void SetFlag(uint flag, bool value)
        {
            if (value)
            {
                Status |= flag;
            }
            else
            {
                Status &= ~flag;
            }
        }

        /// <summary>
        /// The LE line
        /// </summary>
        public bool SignalLE
        {
            get => GetFlag(LinuxConstants.TIOCM_LE) == LinuxConstants.TIOCM_LE;
            set => SetFlag(LinuxConstants.TIOCM_LE, value);
        }

        /// <summary>
        /// The DTR line
        /// </summary>
        public bool SignalDTR
        {
            get => GetFlag(LinuxConstants.TIOCM_DTR) == LinuxConstants.TIOCM_DTR;
            set => SetFlag(LinuxConstants.TIOCM_DTR, value);
        }

        /// <summary>
        /// The RTS line
        /// </summary>
        public bool SignalRTS
        {
            get => GetFlag(LinuxConstants.TIOCM_RTS) == LinuxConstants.TIOCM_RTS;
            set => SetFlag(LinuxConstants.TIOCM_RTS, value);
        }

        /// <summary>
        /// The ST line
        /// </summary>
        public bool SignalST
        {
            get => GetFlag(LinuxConstants.TIOCM_ST) == LinuxConstants.TIOCM_ST;
            set => SetFlag(LinuxConstants.TIOCM_ST, value);
        }

        /// <summary>
        /// The SR line
        /// </summary>
        public bool SignalSR
        {
            get => GetFlag(LinuxConstants.TIOCM_SR) == LinuxConstants.TIOCM_SR;
            set => SetFlag(LinuxConstants.TIOCM_SR, value);
        }

        /// <summary>
        /// The CTS line
        /// </summary>
        public bool SignalCTS
        {
            get => GetFlag(LinuxConstants.TIOCM_CTS) == LinuxConstants.TIOCM_CTS;
            set => SetFlag(LinuxConstants.TIOCM_CTS, value);
        }

        /// <summary>
        /// The CD line
        /// </summary>
        public bool SignalCD
        {
            get => GetFlag(LinuxConstants.TIOCM_CD) == LinuxConstants.TIOCM_CD;  // Same as TIOCM_CAR
            set => SetFlag(LinuxConstants.TIOCM_CD, value);
        }

        /// <summary>
        /// The RNG line
        /// </summary>
        public bool SignalRNG
        {
            get => GetFlag(LinuxConstants.TIOCM_RNG) == LinuxConstants.TIOCM_RNG;    // Same as TIOCM_RI
            set => SetFlag(LinuxConstants.TIOCM_RNG, value);
        }

        /// <summary>
        /// The DSR line
        /// </summary>
        public bool SignalDSR
        {
            get => GetFlag(LinuxConstants.TIOCM_DSR) == LinuxConstants.TIOCM_DSR;
            set => SetFlag(LinuxConstants.TIOCM_DSR, value);
        }

        /// <summary>
        /// The OUT1 line
        /// </summary>
        public bool SignalOUT1
        {
            get => GetFlag(LinuxConstants.TIOCM_OUT1) == LinuxConstants.TIOCM_OUT1;
            set => SetFlag(LinuxConstants.TIOCM_OUT1, value);
        }

        /// <summary>
        /// The OUT2 line
        /// </summary>
        public bool SignalOUT2
        {
            get => GetFlag(LinuxConstants.TIOCM_OUT2) == LinuxConstants.TIOCM_OUT2;
            set => SetFlag(LinuxConstants.TIOCM_OUT2, value);
        }

        /// <summary>
        /// The LOOP line
        /// </summary>
        public bool SignalLOOP
        {
            get => GetFlag(LinuxConstants.TIOCM_LOOP) == LinuxConstants.TIOCM_LOOP;
            set => SetFlag(LinuxConstants.TIOCM_LOOP, value);
        }

    }
}
