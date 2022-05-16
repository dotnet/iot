// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// The minimal API surface exposed to serial port writers
    /// </summary>
    public interface ISerialWriter
    {
        /// <summary>
        /// Returns the number of bytes that are still in the serial port send buffer
        /// </summary>
        int BytesToWrite { get; }

        /// <summary>
        /// Synchronously flush the serial port writing buffer
        /// </summary>
        void Flush();

        /// <summary>
        /// Asynchronously flush the serial port writing buffer
        /// </summary>
        Task FlushAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Send a single byte to the serial port
        /// </summary>
        void WriteByte(byte value);

        /// <summary>
        /// Synchronously send a binary buffer to the serial port
        /// </summary>
        void Write(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Asynchronously send a binary buffer to the serial port
        /// </summary>
        Task WriteAsync(ReadOnlySpan<byte> buffer, CancellationToken cancellation = default(CancellationToken));
    }
}
