// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// The base Stream class implementing some properties and methods
    /// that are common for any serial port transport
    /// </summary>
    public class SerialStream : Stream
    {
        private byte[] _oneByteBuffer = new byte[1];

        /// <summary>
        /// The current serial port instance
        /// </summary>
        protected readonly SerialPort _serialPort;

        /// <summary>
        /// Creates a new instance of the stream managing the Serial Port reads and writes
        /// </summary>
        /// <param name="serialPort">A valid serial port instance</param>
        public SerialStream(SerialPort serialPort)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException(nameof(serialPort));
            }

            _serialPort = serialPort;
        }

        /// <summary>
        /// Returns false as serial communication is not seekable
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Returns true according to the serial port timeout values
        /// </summary>
        public override bool CanTimeout => true;

        /// <summary>
        /// True when the serial port is opened
        /// </summary>
        public override bool CanRead => _serialPort.IsOpen;

        /// <summary>
        /// True when the serial port is closed
        /// </summary>
        public override bool CanWrite => _serialPort.IsOpen;

        /// <summary>
        /// This property throws because the serial port stream is not seekable
        /// </summary>
        public override long Length => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        /// <summary>
        /// This property throws because the serial port stream is not seekable
        /// </summary>
        public override long Position
        {
            get => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);
            set => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);
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
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead => _serialPort.BytesToRead;

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite => _serialPort.BytesToWrite;

        /// <summary>
        /// This method throws because the serial port stream is not seekable
        /// </summary>
        public override void SetLength(long value)
            => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        /// <summary>
        /// This method throws because the serial port stream is not seekable
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        /// <summary>
        /// This method is used to close the stream, but not the serial port
        /// </summary>
        public override void Close()
        {
            // base.Dispose calls Close()
            // base.Close class Dispose(true) and GC.SuppressFinalize.
            base.Close();
        }

        /// <summary>
        /// This method is used to dispose the stream, but not the serial port
        /// </summary>
        public override ValueTask DisposeAsync()
        {
            // base.DisposeAsync calls Dispose() synchronously
            return base.DisposeAsync();
        }

        /// <summary>
        /// This method is used to dispose the stream, but not the serial port
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // base call is empty
        }

        /// <summary>
        /// Reads a single byte from the serial port
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            // TODO: implement this and remove the call to the base
            // return base.ReadByte();
            _oneByteBuffer[0] = 0;
            return _serialPort.Read(_oneByteBuffer, 0, 1);
        }

        /// <summary>
        /// Reads data from the serial port into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer receiving the read data</param>
        /// <param name="offset">The offset where the data will be written to</param>
        /// <param name="count">The maximum amount of bytes to read</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // TODO: implement this and remove the call to the base
            // throw new NotImplementedException();
            return _serialPort.Read(buffer, offset, count);
        }

        /// <summary>
        /// Writes a single byte to the serial port
        /// </summary>
        /// <param name="value">The byte to write</param>
        public override void WriteByte(byte value)
        {
            // TODO: implement this and remove the call to the base
            // base.WriteByte(value);
            _oneByteBuffer[0] = value;
            _serialPort.Write(_oneByteBuffer, 0, 1);
        }

        /// <summary>
        /// Writes the data in the buffer to the serial port
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="offset">The offset to the beginning of data to write</param>
        /// <param name="count">The total amount of bytes to write</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // TODO: implement this and remove the call to the base
            // throw new NotImplementedException();
            _serialPort.Write(buffer, offset, count);
        }

        #region Can be entirely removed

        /// <summary>
        /// Reads data from the serial port into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer receiving the read data</param>
        /// <returns></returns>
        public override int Read(Span<byte> buffer)
        {
            // base is ok, can remove the override
            return base.Read(buffer);
        }

        /// <summary>
        /// Reads data from the serial port into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer receiving the read data</param>
        /// <param name="offset">The offset where the data will be written to</param>
        /// <param name="count">The maximum amount of bytes to read</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // base is ok, can remove the override
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Reads data from the serial port into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer receiving the read data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // base is ok, can remove the override
            return base.ReadAsync(buffer, cancellationToken);
        }

        /// <summary>
        /// Writes the data in the buffer to the serial port
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // base is ok, can remove the override
            base.Write(buffer);
        }

        /// <summary>
        /// Writes the data in the buffer to the serial port
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="offset">The offset to the beginning of data to write</param>
        /// <param name="count">The total amount of bytes to write</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // base is ok, can remove the override
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Writes the data in the buffer to the serial port
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // base is ok, can remove the override
            return base.WriteAsync(buffer, cancellationToken);
        }

        /// <summary>
        /// Starts an asynchronous read from the serial port into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer receiving the read data</param>
        /// <param name="offset">The offset where the data will be written to</param>
        /// <param name="count">The maximum amount of bytes to read</param>
        /// <param name="callback">The callback to call as soon as the operation concludes</param>
        /// <param name="state">The user defined state to pass from the begin to the end of the asynchronus call</param>
        /// <returns></returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            // base is ok, can remove the override
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Ends an asynchronous read from the serial port into the provided buffer
        /// </summary>
        public override int EndRead(IAsyncResult asyncResult)
        {
            // base is ok, can remove the override
            return base.EndRead(asyncResult);
        }

        /// <summary>
        /// Starts an asynchronous write to the serial port
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="offset">The offset to the beginning of data to write</param>
        /// <param name="count">The total amount of bytes to write</param>
        /// <param name="callback">The callback to call as soon as the operation concludes</param>
        /// <param name="state">The user defined state to pass from the begin to the end of the asynchronus call</param>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            // base is ok, can remove the override
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Ends an asynchronous write to the serial port
        /// </summary>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            // base is ok, can remove the override
            base.EndWrite(asyncResult);
        }

        #endregion

        /// <summary>
        /// Flusn the buffer to the serial port
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void Flush()
        {
            // throw new NotImplementedException();
            _serialPort.Flush();
        }

        /// <summary>
        /// Asynchronously flush the buffer to the serial port
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return base.FlushAsync(cancellationToken);
        }

    }
}
