// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Standard Identifier Low Register.
    /// </summary>
    public class RxMxSidl
    {
        /// <summary>
        /// Initializes a new instance of the RxMxSidl class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="eid">Extended Identifier Mask bits.</param>
        /// <param name="sid">Standard Identifier Mask bits.</param>
        public RxMxSidl(RxMaskNumber rxMaskNumber, byte eid, byte sid)
        {
            RxMaskNumber = rxMaskNumber;
            Eid = eid;
            Sid = sid;
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; set; }

        /// <summary>
        /// Extended Identifier Mask bits.
        /// </summary>
        public byte Eid { get; set; }

        /// <summary>
        /// Standard Identifier Mask bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
