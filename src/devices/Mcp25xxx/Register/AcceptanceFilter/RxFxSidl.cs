// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Standard Identifier Low Register.
    /// </summary>
    public class RxFxSidl
    {
        /// <summary>
        /// Initializes a new instance of the RxFxSidl class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number.</param>
        /// <param name="eid">Extended Identifier Filter bits.</param>
        /// <param name="exide">
        /// Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </param>
        /// <param name="sid">Standard Identifier Filter bits.</param>
        public RxFxSidl(RxFilterNumber rxFilterNumber, byte eid, bool exide, byte sid)
        {
            RxFilterNumber = rxFilterNumber;
            Eid = eid;
            Exide = exide;
            Sid = sid;
        }

        /// <summary>
        /// Receive Filter Number.
        /// </summary>
        public RxFilterNumber RxFilterNumber { get; set; }

        /// <summary>
        /// Extended Identifier Filter bits.
        /// </summary>
        public byte Eid { get; set; }

        /// <summary>
        /// Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </summary>
        public bool Exide { get; set; }

        /// <summary>
        /// Standard Identifier Filter bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
