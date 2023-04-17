// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Errors from the NMEA parser
    /// </summary>
    public enum NmeaError
    {
        /// <summary>
        /// Parsing was successful
        /// </summary>
        None = 0,

        /// <summary>
        /// A message was shorter than the minimal message size
        /// </summary>
        MessageToShort,

        /// <summary>
        /// The message checksum did not match.
        /// A missing checksum will not be reported as error
        /// </summary>
        InvalidChecksum,

        /// <summary>
        /// The message is to long
        /// </summary>
        MessageToLong,

        /// <summary>
        /// There was no sync byte found
        /// </summary>
        NoSyncByte,

        /// <summary>
        /// The communication stream is closed, no more data can be written
        /// </summary>
        PortClosed,

        /// <summary>
        /// The message processing was delayed. A possible reason for this is if attempting
        /// to send to much data over a slow serial link.
        /// </summary>
        MessageDelayed,

        /// <summary>
        /// A message was dropped, because a newer message was already in the queue. This indicates an output buffer overrun.
        /// </summary>
        MessageDropped
    }
}
