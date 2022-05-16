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
    /// The minimal API surface exposed to serial port readers
    /// </summary>
    public interface ISerialReader
    {
        /// <summary>
        /// Returns the number of available bytes in the serial port buffer
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        /// Read a single byte from the serial port
        /// </summary>
        /// <returns></returns>
        byte ReadByte();

        /// <summary>
        /// Synchronously read the content of the serial port into the provided buffer
        /// </summary>
        int Read(Span<byte> buffer);

        /// <summary>
        /// Asynchronously read the content of the serial port into the provided buffer
        /// </summary>
        Task<int> ReadAsync(Span<byte> buffer, CancellationToken cancellation = default(CancellationToken));

        /*
         * ReadUntil / ReadTo
        */
    }
}
