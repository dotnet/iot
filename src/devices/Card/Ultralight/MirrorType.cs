// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// The type of mirror activated
    /// </summary>
    [Flags]
    public enum MirrorType
    {
        /// <summary>
        /// No mirror activated
        /// </summary>
        None = 0,

        /// <summary>
        /// UID ASCII activated
        /// </summary>
        UidAscii = 1,

        /// <summary>
        /// NFC counter activated
        /// </summary>
        NfcCounter = 2,
    }
}
