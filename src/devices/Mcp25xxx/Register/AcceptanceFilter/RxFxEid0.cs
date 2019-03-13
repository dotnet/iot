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
        /// <param name="rxFilterNumber">Receive Filter Number.  Ranges 0 - 5.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxFxEid0(byte rxFilterNumber, byte eid)
        {
            if (rxFilterNumber > 5)
            {
                throw new ArgumentException($"Invalid RX Filter Number value {rxFilterNumber}.", nameof(rxFilterNumber));
            }

            RxFilterNumber = rxFilterNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Filter Number.  Ranges 0 - 5.
        /// </summary>
        public byte RxFilterNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case 0:
                    return Address.RxF0Eid0;
                case 1:
                    return Address.RxF1Eid0;
                case 2:
                    return Address.RxF2Eid0;
                case 3:
                    return Address.RxF3Eid0;
                case 4:
                    return Address.RxF4Eid0;
                case 5:
                    return Address.RxF5Eid0;
                default:
                    throw new ArgumentException($"Invalid Rx Filter Number value {RxFilterNumber}.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static byte GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Eid0:
                    return 0;
                case Address.RxF1Eid0:
                    return 1;
                case Address.RxF2Eid0:
                    return 2;
                case Address.RxF3Eid0:
                    return 3;
                case Address.RxF4Eid0:
                    return 4;
                case Address.RxF5Eid0:
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
        public byte ToByte() => Eid;
    }
}
