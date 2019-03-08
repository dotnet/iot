// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Standard Identifier High Register.
    /// </summary>
    public class RxBxSidh : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxBxSidh class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="sid">Standard Identifier bits.</param>
        public RxBxSidh(RxBufferNumber rxBufferNumber, byte sid)
        {
            RxBufferNumber = rxBufferNumber;
            Sid = sid;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; set; }

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to up look Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static RxBufferNumber GetRxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.RxB0Sidh:
                    return RxBufferNumber.Zero;
                case Address.RxB1Sidh:
                    return RxBufferNumber.One;
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
            switch (RxBufferNumber)
            {
                case RxBufferNumber.Zero:
                    return Address.RxB0Sidh;
                case RxBufferNumber.One:
                    return Address.RxB1Sidh;
                default:
                    throw new ArgumentException("Invalid Rx Buffer Number.", nameof(RxBufferNumber));
            }
        }

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => Sid;
    }
}
