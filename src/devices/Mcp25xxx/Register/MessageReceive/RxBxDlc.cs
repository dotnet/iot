// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Data Length Code Register.
    /// </summary>
    public class RxBxDlc : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxBxDlc class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="dlc">
        /// Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </param>
        /// <param name="rtr">
        /// Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </param>
        public RxBxDlc(RxBufferNumber rxBufferNumber, byte dlc, bool rtr)
        {
            RxBufferNumber = rxBufferNumber;
            Dlc = dlc;
            Rtr = rtr;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </summary>
        public byte Dlc { get; set; }

        /// <summary>
        /// Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </summary>
        public bool Rtr { get; set; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case RxBufferNumber.Zero:
                    return Address.RxB0Dlc;
                case RxBufferNumber.One:
                    return Address.RxB1Dlc;
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
                case Address.RxB0Dlc:
                    return RxBufferNumber.Zero;
                case Address.RxB1Dlc:
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
        public byte ToByte()
        {
            byte value = 0;

            if (Rtr)
            {
                value |= 0b100_0000;
            }

            value |= Dlc;
            return value;
        }
    }
}
