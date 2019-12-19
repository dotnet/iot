// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.ListPassive
{
    /// <summary>
    /// The baud rate target for specific cards
    /// </summary>
    public enum TargetBaudRate
    {
        /// <summary>
        ///  106 kbps type A (ISO/IEC14443 Type A)
        /// </summary>
        B106kbpsTypeA = 0x00,

        /// <summary>
        /// 212 kbps (FeliCa polling)
        /// </summary>
        B212kbps = 0x01,

        /// <summary>
        /// 424 kbps (FeliCa polling)
        /// </summary>
        B424kbps = 0x02,

        /// <summary>
        /// 106 kbps type B (ISO/IEC14443-3B)
        /// </summary>
        B106kbpsTypeB = 0x03,

        /// <summary>
        /// 106 kbps Innovision Jewel tag.
        /// </summary>
        B106kbpsInnovisionJewelTag = 0x04
    }
}
