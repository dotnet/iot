// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// PN532 firmware version
    /// </summary>
    public class FirmwareVersion
    {
        /// <summary>
        /// The identification code for PN532 should be 0x32
        /// </summary>
        public byte IdentificationCode { get; set; }

        /// <summary>
        /// The version, lastest know one is 1.6
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// All card version supported
        /// </summary>
        public VersionSupported VersionSupported { get; set; }

        /// <summary>
        /// Is it a PN532?
        /// </summary>
        public bool IsPn532 => IdentificationCode == 0x32;
    }
}
