// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Extended Identifier High Register.
    /// </summary>
    public class RxMxEid8 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxMxEid8 class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxMxEid8(RxMaskNumber rxMaskNumber, byte eid)
        {
            RxMaskNumber = rxMaskNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }

        private Address GetAddress()
        {
            switch (RxMaskNumber)
            {
                case RxMaskNumber.Zero:
                    return Address.RxM0Eid8;
                case RxMaskNumber.One:
                    return Address.RxM1Eid8;
                default:
                    throw new ArgumentException("Invalid Rx Mask Number.", nameof(RxMaskNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Mask Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Mask Number.</param>
        /// <returns>The Rx Mask Number based on the register address.</returns>
        public static RxMaskNumber GetRxMaskNumber(Address address)
        {
            switch (address)
            {
                case Address.RxM0Eid8:
                    return RxMaskNumber.Zero;
                case Address.RxM1Eid8:
                    return RxMaskNumber.One;
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
