// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Extended Identifier High Register.
    /// </summary>
    public class RxBxEid8 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxBxEid8 class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxBxEid8(RxBufferNumber rxBufferNumber, byte eid)
        {
            RxBufferNumber = rxBufferNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case RxBufferNumber.Zero:
                    return Address.RxB0Eid8;
                case RxBufferNumber.One:
                    return Address.RxB1Eid8;
                default:
                    throw new ArgumentException("Invalid Rx Buffer Number.", nameof(RxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static RxBufferNumber GetRxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.RxB0Eid8:
                    return RxBufferNumber.Zero;
                case Address.RxB1Eid8:
                    return RxBufferNumber.One;
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
