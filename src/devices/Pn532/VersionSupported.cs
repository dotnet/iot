// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// All supported version for the NFC reader
    /// TODO: if more readers appear, this can be for sure place
    /// in common and more modes can be added
    /// </summary>
    [Flags]
    public enum VersionSupported
    {
        /// <summary>
        /// Iso 18092
        /// </summary>
        Iso18092 = 0b0000_0100,

        /// <summary>
        /// Iso/Iec 14443 Type B
        /// </summary>
        IsoIec14443TypeB = 0b0000_0010,

        /// <summary>
        /// Iso/Iec 14443 Type A
        /// </summary>
        IsoIec14443TypeA = 0b0000_0001
    }
}
