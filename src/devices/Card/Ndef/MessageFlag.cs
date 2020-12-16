// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Message flag
    /// </summary>
    [Flags]
    public enum MessageFlag
    {
        /// <summary>
        /// The first record of a NDEF message
        /// </summary>
        MessageBegin = 0b1000_0000,

        /// <summary>
        /// The last record of a NDEF message
        /// </summary>
        MessageEnd = 0b0100_0000,

        /// <summary>
        /// This is part of multi composed record
        /// </summary>
        ChunkFlag = 0b0010_0000,

        /// <summary>
        /// Short message
        /// </summary>
        ShortRecord = 0b0001_0000,

        /// <summary>
        /// Is the id length present or not
        /// </summary>
        IdLength = 0b0000_1000,
    }
}
