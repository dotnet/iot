// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Extended Identifier High Register.
    /// </summary>
    public class RxMxEid8
    {
        /// <summary>
        /// Initializes a new instance of the RxMxEid8 class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxMxEid8(RxMaskNumber rxMaskNumber, byte eid)
        {
            RxMaskNumber = rxMaskNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }
    }
}
