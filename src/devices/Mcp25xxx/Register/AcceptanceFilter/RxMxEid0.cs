// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Extended Identifier Low Register.
    /// </summary>
    public class RxMxEid0
    {
        /// <summary>
        /// Initializes a new instance of the RxMxEid0 class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="eid">Extended Identifier Mask bits.</param>
        public RxMxEid0(RxMaskNumber rxMaskNumber, byte eid)
        {
            RxMaskNumber = rxMaskNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; set; }

        /// <summary>
        /// Extended Identifier Mask bits.
        /// </summary>
        public byte Eid { get; set; }
    }
}
