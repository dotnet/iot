// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// The class that allows to exchange data through the serial port
    /// with no knowledge about the serial port details
    /// </summary>
    public class SerialStreamTransport : SerialStreamBase, ISerialReader, ISerialWriter
    {
        private readonly SerialPort _serialPort;

        internal SerialStreamTransport(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        public override int ReadTimeout
        {
            get => _serialPort.ReadTimeout;
            set => _serialPort.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        public override int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }

        /// <summary>
        /// True when the serial port is opened
        /// </summary>
        public override bool CanRead => _serialPort.IsOpen;

        /// <summary>
        /// True when the serial port is closed
        /// </summary>
        public override bool CanWrite => _serialPort.IsOpen;

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead => _serialPort.BytesToRead;

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite => _serialPort.BytesToWrite;

        /// <summary>
        /// todo
        /// </summary>
        /// <returns>todo</returns>
        public override int ReadByte()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="buffer">todo</param>
        /// <returns>todo</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override int Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="buffer">todo</param>
        /// <param name="cancellation">todo</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">todo</exception>
        public Task<int> ReadAsync(Span<byte> buffer, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="cancellationToken">todo</param>
        /// <returns></returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="value">todo</param>
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="buffer">todo</param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="buffer">todo</param>
        /// <param name="cancellation">todo</param>
        /// <returns></returns>
        public Task WriteAsync(ReadOnlySpan<byte> buffer, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}
