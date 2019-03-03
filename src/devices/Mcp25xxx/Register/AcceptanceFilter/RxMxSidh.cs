// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Standard Identifier High Register.
    /// </summary>
    public class RxMxSidh
    {
        /// <summary>
        /// Initializes a new instance of the RxMxSidh class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="sid">Standard Identifier Mask bits.</param>
        public RxMxSidh(RxMaskNumber rxMaskNumber, byte sid)
        {
            RxMaskNumber = rxMaskNumber;
            Sid = sid;
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; set; }

        /// <summary>
        /// Standard Identifier Mask bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
