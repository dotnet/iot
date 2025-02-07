// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    ///  Different storage capacity for ICODE cards
    ///  https://www.nxp.com.cn/docs/en/application-note/AN11809.pdf
    /// </summary>
    public enum IcodeCardCapacity
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ICODE SLIX
        /// </summary>
        ICODESLIX = 896,

        /// <summary>
        /// ICODE SLIX2
        /// </summary>
        ICODESLIX2 = 2528,

        /// <summary>
        /// ICODE DNA
        /// </summary>
        ICODEDNA = 2016,

        /// <summary>
        /// ICODE 3
        /// </summary>
        ICODE3 = 2400,

        /// <summary>
        /// ICODE 3 TagTamper
        /// </summary>
        ICODE3TagTamper = 2400
    }
}
