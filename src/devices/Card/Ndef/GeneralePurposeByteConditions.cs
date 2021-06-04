// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// This is used to assess the block sector and understand the read/write conditions
    /// </summary>
    [Flags]
    public enum GeneralePurposeByteConditions
    {
        /// <summary>
        /// No read access
        /// </summary>
        NoReadAccess = 0b0000_1100,

        /// <summary>
        /// No write access
        /// </summary>
        NoWriteAccess = 0b0000_0011,

        /// <summary>
        /// Read access
        /// </summary>
        ReadAccess = 0b0000_0000,

        /// <summary>
        /// Write access
        /// </summary>
        WriteAccess = 0b0000_0000,
    }
}
