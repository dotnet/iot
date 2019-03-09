// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Standard Identifier High Register.
    /// </summary>
    public class RxFxSidh : IRegister
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
        public RxFilterNumber RxFilterNumber { get; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case RxFilterNumber.Zero:
                    return Address.RxF0Sidh;
                case RxFilterNumber.One:
                    return Address.RxF1Sidh;
                case RxFilterNumber.Two:
                    return Address.RxF2Sidh;
                case RxFilterNumber.Three:
                    return Address.RxF3Sidh;
                case RxFilterNumber.Four:
                    return Address.RxF4Sidh;
                case RxFilterNumber.Five:
                    return Address.RxF5Sidh;
                default:
                    throw new ArgumentException("Invalid Rx Filter Number.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to up look Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static RxFilterNumber GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Sidh:
                    return RxFilterNumber.Zero;
                case Address.RxF1Sidh:
                    return RxFilterNumber.One;
                case Address.RxF2Sidh:
                    return RxFilterNumber.Two;
                case Address.RxF3Sidh:
                    return RxFilterNumber.Three;
                case Address.RxF4Sidh:
                    return RxFilterNumber.Four;
                case Address.RxF5Sidh:
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
        public byte ToByte() => Sid;
    }
}
