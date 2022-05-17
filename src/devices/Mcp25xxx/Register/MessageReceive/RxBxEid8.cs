// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="extendedIdentifier">
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold bits 15 through 8 of the Extended Identifier for the received message.
        /// </param>
        public RxBxEid8(byte rxBufferNumber, byte extendedIdentifier)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            ExtendedIdentifier = extendedIdentifier;
        }

        /// <summary>
        /// Receive Buffer Number. Must be a value of 0 - 1.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold bits 15 through 8 of the Extended Identifier for the received message.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        private Address GetAddress() => RxBufferNumber switch
        {
            0 => Address.RxB0Eid8,
            1 => Address.RxB1Eid8,
            _ => throw new Exception($"Invalid value for {nameof(RxBufferNumber)}: {RxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static byte GetRxBufferNumber(Address address) => address switch
        {
            Address.RxB0Eid8 => 0,
            Address.RxB1Eid8 => 1,
            _ => throw new ArgumentException($"Invalid address value: {address}.", nameof(address)),
        };

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => GetAddress();

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => ExtendedIdentifier;
    }
}
