// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Extended Identifier High Register.
    /// </summary>
    public class RxFxEid8 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxFxEid8 class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxFxEid8(RxFilterNumber rxFilterNumber, byte eid)
        {
            RxFilterNumber = rxFilterNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Filter Number.
        /// </summary>
        public RxFilterNumber RxFilterNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case RxFilterNumber.Zero:
                    return Address.RxF0Eid8;
                case RxFilterNumber.One:
                    return Address.RxF1Eid8;
                case RxFilterNumber.Two:
                    return Address.RxF2Eid8;
                case RxFilterNumber.Three:
                    return Address.RxF3Eid8;
                case RxFilterNumber.Four:
                    return Address.RxF4Eid8;
                case RxFilterNumber.Five:
                    return Address.RxF5Eid8;
                default:
                    throw new ArgumentException("Invalid Rx Filter Number.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static RxFilterNumber GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Eid8:
                    return RxFilterNumber.Zero;
                case Address.RxF1Eid8:
                    return RxFilterNumber.One;
                case Address.RxF2Eid8:
                    return RxFilterNumber.Two;
                case Address.RxF3Eid8:
                    return RxFilterNumber.Three;
                case Address.RxF4Eid8:
                    return RxFilterNumber.Four;
                case Address.RxF5Eid8:
                    return RxFilterNumber.Five;
                default:
                    throw new ArgumentException("Invalid address.", nameof(address));
            }
        }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => GetAddress();

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => Eid;
    }
}
