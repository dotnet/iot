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
        /// <param name="rxFilterNumber">Receive Filter Number.  Ranges 0 - 5.</param>
        /// <param name="sid">Standard Identifier bits.</param>
        public RxFxSidh(byte rxFilterNumber, byte sid)
        {
            if (rxFilterNumber > 5)
            {
                throw new ArgumentException($"Invalid RX Filter Number value {rxFilterNumber}.", nameof(rxFilterNumber));
            }

            RxFilterNumber = rxFilterNumber;
            Sid = sid;
        }

        /// <summary>
        /// Receive Filter Number.  Ranges 0 - 5.
        /// </summary>
        public byte RxFilterNumber { get; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case 0:
                    return Address.RxF0Sidh;
                case 1:
                    return Address.RxF1Sidh;
                case 2:
                    return Address.RxF2Sidh;
                case 3:
                    return Address.RxF3Sidh;
                case 4:
                    return Address.RxF4Sidh;
                case 5:
                    return Address.RxF5Sidh;
                default:
                    throw new ArgumentException($"Invalid Rx Filter Number value {RxFilterNumber}.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to up look Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static byte GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Sidh:
                    return 0;
                case Address.RxF1Sidh:
                    return 1;
                case Address.RxF2Sidh:
                    return 2;
                case Address.RxF3Sidh:
                    return 3;
                case Address.RxF4Sidh:
                    return 4;
                case Address.RxF5Sidh:
                    return 5;
                default:
                    throw new ArgumentException($"Invalid address value {address}.", nameof(address));
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
