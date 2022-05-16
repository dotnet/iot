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
    internal class SerialStreamBase : Stream
    {
        public override bool CanSeek => false;

        public override bool CanTimeout => true;

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        public override long Position
        {
            get => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);
            set => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);
        }

        public override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }

        public override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

        public override void SetLength(long value)
            => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException(Strings.NotSupported_UnseekableStream);

        public override void Close()
        {
            // base.Dispose calls Close()
            // base.Close class Dispose(true) and GC.SuppressFinalize.
            base.Close();
        }

        public override ValueTask DisposeAsync()
        {
            // base.DisposeAsync calls Dispose() synchronously
            return base.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            // base call is empty
        }

        public override int ReadByte()
        {
            // TODO: implement this and remove the call to the base
            return base.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // TODO: implement this and remove the call to the base
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            // TODO: implement this and remove the call to the base
            base.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // TODO: implement this and remove the call to the base
            throw new NotImplementedException();
        }

        #region Can be entirely removed
        public override int Read(Span<byte> buffer)
        {
            // base is ok, can remove the override
            return base.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // base is ok, can remove the override
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // base is ok, can remove the override
            return base.ReadAsync(buffer, cancellationToken);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // base is ok, can remove the override
            base.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // base is ok, can remove the override
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // base is ok, can remove the override
            return base.WriteAsync(buffer, cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            // base is ok, can remove the override
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            // base is ok, can remove the override
            return base.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            // base is ok, can remove the override
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            // base is ok, can remove the override
            base.EndWrite(asyncResult);
        }

        #endregion

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return base.FlushAsync(cancellationToken);
        }

    }
}
