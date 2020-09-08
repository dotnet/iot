// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ndef
{
    /// <summary>
    /// The format type
    /// </summary>
    public enum TypeNameFormat
    {
        /// <summary>
        /// Empty
        /// </summary>
        Empty = 0x00,

        /// <summary>
        /// NFC Forum well-known type [NFC RTD]
        /// </summary>
        NfcWellKnowType = 0x01,

        /// <summary>
        /// Media-type as defined in RFC 2046 [RFC 2046]
        /// </summary>
        MediaType = 0x02,

        /// <summary>
        /// Absolute URI as defined in RFC 3986 [RFC 3986]
        /// </summary>
        UniformResourceIdentifier = 0x03,

        /// <summary>
        /// NFC Forum external type [NFC RTD]
        /// </summary>
        NfcForumExpernal = 0x04,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0x05,

        /// <summary>
        /// Unchanged (see section 2.3.3)
        /// </summary>
        Unchanged = 0x06,

        /// <summary>
        /// Reserved
        /// </summary>
        Reserved = 0x07,
    }
}
