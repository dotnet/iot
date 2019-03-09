// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Extended Identifier High Register.
    /// </summary>
    public class TxBxEid8 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxEid8 class.
        /// </summary>
        /// <param name="txBufferNumber"> Transmit Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public TxBxEid8(TxBufferNumber txBufferNumber, byte eid)
        {
            TxBufferNumber = txBufferNumber;
            Eid = eid;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static TxBufferNumber GetTxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.TxB0Eid8:
                    return TxBufferNumber.Zero;
                case Address.TxB1Eid8:
                    return TxBufferNumber.One;
                case Address.TxB2Eid8:
                    return TxBufferNumber.Two;
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
            switch (TxBufferNumber)
            {
                case TxBufferNumber.Zero:
                    return Address.TxB0Eid8;
                case TxBufferNumber.One:
                    return Address.TxB1Eid8;
                case TxBufferNumber.Two:
                    return Address.TxB2Eid8;
                default:
                    throw new ArgumentException("Invalid Tx Buffer Number.", nameof(TxBufferNumber));
            }
        }

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => Eid;
    }
}
