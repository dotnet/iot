// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Ports.SerialPort.Resources;
using System.Text;

/*
// Rationale

// This class maintain an immutable set of parameters.
// I decided to avoid storing the port name because it happens quite often
// that the port name changes depending on the physical usb receptacle used
// to plug-in the device.
//
// The communication parameters are almost always never changed.
// Anyway, there are times where the user may want to change the baud rate
// or even the parity (this is the trick used to support 9 data bit communication).
// In these cases the user just creates two or more settings and pass them to
// the serial port class to change them accordingly.

Handshake
ParityReplace
BreakState
DiscardNull
ReadTimeout
WriteTimeout

DtrEnable
RtsEnable

CDHolding
CtsHolding
DsrHolding

*/

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Represent the communication settings for the serial communication
    /// </summary>
    public class SerialPortSettings
    {
        private const int MaxDataBitsNoParity = 9;
        private const int MinDataBits = 5;

        private const int DefaultBaudRate = 9600;
        private const Parity DefaultParity = Parity.None;
        private const int DefaultDataBits = 8;
        private const StopBits DefaultStopBits = StopBits.One;
        private const Handshake DefaultHandshake = Handshake.None;

        /// <summary>
        /// Set the settings using the default values: 9600, N, 8, 1
        /// </summary>
        public SerialPortSettings()
            : this(DefaultBaudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
        {
        }

        /// <summary>
        /// Set the settings using the default values: <paramref name="baudRate"/>, N, 8, 1
        /// </summary>
        /// <param name="baudRate">The baud rate</param>
        public SerialPortSettings(int baudRate)
            : this(baudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
        {
        }

        /// <summary>
        /// Set the settings using the default values: <paramref name="baudRate"/>,
        /// <paramref name="parity"/>, 8, 1
        /// </summary>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        public SerialPortSettings(int baudRate, Parity parity)
            : this(baudRate, parity, DefaultDataBits, DefaultStopBits)
        {
        }

        /// <summary>
        /// Set the settings using the default values: <paramref name="baudRate"/>,
        /// <paramref name="parity"/>, <paramref name="dataBits"/>, 1
        /// </summary>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        /// <param name="dataBits">The data bits in the [5,9] interval</param>
        public SerialPortSettings(int baudRate, Parity parity, int dataBits)
            : this(baudRate, parity, dataBits, DefaultStopBits)
        {
        }

        /// <summary>
        /// Set the settings using the default values: <paramref name="baudRate"/>,
        /// <paramref name="parity"/>, <paramref name="dataBits"/>, <paramref name="stopBits"/>
        /// </summary>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        /// <param name="dataBits">The data bits in the [5,9] interval</param>
        /// <param name="stopBits">The stop bits</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SerialPortSettings(int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            if (baudRate <= 0)
            {
                // BaudRate values strictly depends from the serial port driver
                // Since modern hardware does support very high speeds, the values are not limited
                throw new ArgumentOutOfRangeException(nameof(BaudRate), Strings.ArgumentOutOfRange_NeedPosNum);
            }

            BaudRate = baudRate;
            Parity = parity;

            // 9 data bit is only supported by cheating the parity bit
            if (dataBits < MinDataBits || dataBits > MaxDataBitsNoParity ||
                (dataBits == MaxDataBitsNoParity && parity != Parity.None))
            {
                throw new ArgumentOutOfRangeException(nameof(dataBits), Strings.InvalidDataBits);
            }

            DataBits = dataBits;
            StopBits = stopBits;
        }

        /// <summary>
        /// The baud rate
        /// </summary>
        public int BaudRate { get; }

        /// <summary>
        /// The parity
        /// </summary>
        public Parity Parity { get; }

        /// <summary>
        /// The data bits
        /// </summary>
        public int DataBits { get; }

        /// <summary>
        /// The stop bits
        /// </summary>
        public StopBits StopBits { get; }
    }
}
