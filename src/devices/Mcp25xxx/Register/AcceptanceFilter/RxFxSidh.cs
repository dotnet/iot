// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Standard Identifier High Register.
    /// </summary>
    public class RxFxSidh
    {
        /// <summary>
        /// Initializes a new instance of the RxFxSidh class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number.</param>
        /// <param name="sid">Standard Identifier bits.</param>
        public RxFxSidh(RxFilterNumber rxFilterNumber, byte sid)
        {
            RxFilterNumber = rxFilterNumber;
            Sid = sid;
        }

        /// <summary>
        /// Receive Filter Number.
        /// </summary>
        public RxFilterNumber RxFilterNumber { get; set; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
