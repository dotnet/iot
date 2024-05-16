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
    public class SerialStreamTransport : SerialStream // , ISerialReader, ISerialWriter
    {
        /// <summary>
        /// Creates a new instance of the stream managing the Serial Port reads and writes
        /// </summary>
        /// <param name="serialPort">A valid serial port instance</param>
        public SerialStreamTransport(SerialPort serialPort)
            : base(serialPort)
        {
        }

        /*
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
        */
    }
}
