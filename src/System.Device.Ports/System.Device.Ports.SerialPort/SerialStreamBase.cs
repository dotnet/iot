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
    public abstract class SerialStreamBase : Stream
    {
        /// <summary>
        /// Returns false as serial communication is not seekable
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Returns true according to the serial port timeout values
        /// </summary>
        public override bool CanTimeout => true;

        /// <summary>
        /// True if this stream can be read
        /// </summary>
        public abstract override bool CanRead { get; }

        /// <summary>
        /// True if this stream can be written
        /// </summary>
        public abstract override bool CanWrite { get; }

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
        /// This property should report the same ReadTimeout of the serial port
        /// </summary>
        public abstract override int ReadTimeout { get; set; }

        /// <summary>
        /// This property should report the same WriteTimeout of the serial port
        /// </summary>
        public abstract override int WriteTimeout { get; set; }

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
            return base.ReadByte();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a single byte to the serial port
        /// </summary>
        /// <param name="value">The byte to write</param>
        public override void WriteByte(byte value)
        {
            // TODO: implement this and remove the call to the base
            base.WriteByte(value);
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
