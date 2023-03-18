// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Card
{
    /// <summary>
    /// NFC protocol
    /// These include standards as well as proprietary command sets, for which transceivers
    /// may have special support. For example, Mifare is conveyed across ISO/IEC 14443-3 (Type A),
    /// and transceivers have built-in support for Mifare authentication commands.
    /// </summary>
    [Flags]
    public enum NfcProtocol
    {
        /// <summary>
        /// Unknown or unspecified
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ISO/IEC 14443-3 (Type A or B)
        /// </summary>
        Iso14443_3 = (1 << 0),

        /// <summary>
        /// ISO/IEC 14443-4 (Type A or B)
        /// </summary>
        Iso14443_4 = (1 << 1),

        /// <summary>
        /// Mifare Classic
        /// Proprietary commands on top of ISO/IEC 14443-3 Type A
        /// </summary>
        Mifare = (1 << 2),

        /// <summary>
        /// Innovision Jewel
        /// </summary>
        Jewel = (1 << 3),

        /// <summary>
        /// JIS X 6319-4. Compatible with FeliCa.
        /// </summary>
        JisX6319_4 = (1 << 4),

        /// <summary>
        /// ISO/IEC 15693
        /// </summary>
        Iso15693 = (1 << 5)
    }
}