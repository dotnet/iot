// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ft200X.Samples
{
    /// <summary>
    /// Handles a sensor's request framing and command-dependent responses over a byte FIFO.
    /// </summary>
    internal interface IFifoSensor
    {
        /// <summary>
        /// Consumes the next request byte and returns any response ready to be queued.
        /// </summary>
        /// <param name="value">The next byte, independent of USB read boundaries.</param>
        /// <returns>
        /// An empty memory region if no response is ready; otherwise a response of any length.
        /// The returned memory remains valid until the next call.
        /// </returns>
        ReadOnlyMemory<byte> ProcessByte(byte value);
    }
}
