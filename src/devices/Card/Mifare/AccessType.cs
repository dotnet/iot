// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// The type of access for the data sectors
    /// </summary>
    [Flags]
    public enum AccessType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0b0000_0000,

        /// <summary>
        /// Read Key A
        /// </summary>
        ReadKeyA = 0b0000_0001,

        /// <summary>
        /// Read Key B
        /// </summary>
        ReadKeyB = 0b0000_0010,

        /// <summary>
        /// Write Key A
        /// </summary>
        WriteKeyA = 0b0000_0100,

        /// <summary>
        /// Write Key B
        /// </summary>
        WriteKeyB = 0b0000_1000,

        /// <summary>
        /// Increment Key A
        /// </summary>
        IncrementKeyA = 0b0001_0000,

        /// <summary>
        /// Increment Key B
        /// </summary>
        IncrementKeyB = 0b0010_0000,

        /// <summary>
        /// Decrement Transfer Restore Key A
        /// </summary>
        DecrementTransferRestoreKeyA = 0b0100_0000,

        /// <summary>
        /// Decrement Transfer Restore Key B
        /// </summary>
        DecrementTransferRestoreKeyB = 0b1000_0000,
    }
}
