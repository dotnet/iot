// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Extended Identifier Low Register.
    /// </summary>
    public class RxFxEid0
    {
        /// <summary>
        /// Initializes a new instance of the RxFxEid0 class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxFxEid0(RxFilterNumber rxFilterNumber, byte eid)
        {
            RxFilterNumber = rxFilterNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Filter Number.
        /// </summary>
        public RxFilterNumber RxFilterNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }
    }
}
