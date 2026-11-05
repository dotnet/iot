// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Provides USB-side payload access to an FTDI I2C slave's receive and transmit FIFOs.
    /// </summary>
    /// <remarks>
    /// An external I2C master initiates bus transfers. This contract does not configure
    /// the slave address or preserve I2C transaction boundaries.
    /// </remarks>
    internal interface IFtI2cSlaveTransport : IDisposable
    {
        /// <summary>
        /// Gets the number of received payload bytes available to the USB host.
        /// </summary>
        uint BytesAvailable { get; }

        /// <summary>
        /// Reads payload bytes received from the external I2C master.
        /// </summary>
        /// <param name="buffer">The destination buffer.</param>
        /// <returns>The actual byte count, which may be smaller than the buffer length on timeout.</returns>
        int Read(Span<byte> buffer);

        /// <summary>
        /// Queues payload bytes for the external I2C master to read.
        /// </summary>
        /// <param name="buffer">The payload to queue over USB.</param>
        /// <returns>The number of bytes accepted by the transport, not necessarily consumed by the master.</returns>
        int Write(ReadOnlySpan<byte> buffer);
    }
}
