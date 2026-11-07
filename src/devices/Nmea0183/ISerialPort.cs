// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Ports;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Abstraction over <see cref="SerialPort"/> to allow testing with mocked/fake transports.
    /// </summary>
    public interface ISerialPort : IDisposable
    {
        /// <summary>
        /// Name of the serial port.
        /// </summary>
        string PortName { get; }

        /// <summary>
        /// Serial port baud rate.
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        /// Serial port parity mode.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// Serial port stop bits.
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        /// Number of data bits.
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        /// Underlying stream for reading and flushing operations.
        /// </summary>
        Stream BaseStream { get; }

        /// <summary>
        /// Opens the serial connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the serial connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Writes bytes to the port.
        /// </summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="count">Number of bytes to write.</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads a byte from the port.
        /// </summary>
        int ReadByte();
    }
}
