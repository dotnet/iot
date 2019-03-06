// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Extended Identifier Low Register.
    /// </summary>
    public class RxFxEid0 : IRegister
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
        public RxFilterNumber RxFilterNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static RxFilterNumber GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Eid0:
                    return RxFilterNumber.Zero;
                case Address.RxF1Eid0:
                    return RxFilterNumber.One;
                case Address.RxF2Eid0:
                    return RxFilterNumber.Two;
                case Address.RxF3Eid0:
                    return RxFilterNumber.Three;
                case Address.RxF4Eid0:
                    return RxFilterNumber.Four;
                case Address.RxF5Eid0:
                    return RxFilterNumber.Five;
                default:
                    throw new ArgumentException("Invalid address.", nameof(address));
            }
        }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case RxFilterNumber.Zero:
                    return Address.RxF0Eid0;
                case RxFilterNumber.One:
                    return Address.RxF1Eid0;
                case RxFilterNumber.Two:
                    return Address.RxF2Eid0;
                case RxFilterNumber.Three:
                    return Address.RxF3Eid0;
                case RxFilterNumber.Four:
                    return Address.RxF4Eid0;
                case RxFilterNumber.Five:
                    return Address.RxF5Eid0;
                default:
                    throw new ArgumentException("Invalid Rx Filter Number.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => Eid;
    }
}
