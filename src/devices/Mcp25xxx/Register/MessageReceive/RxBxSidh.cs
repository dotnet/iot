// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="standardIdentifier">
        /// SID[10:3]: Standard Identifier bits.
        /// These bits contain the eight Most Significant bits of the Standard Identifier for the received message.
        /// </param>
        public RxBxSidh(byte rxBufferNumber, byte standardIdentifier)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            StandardIdentifier = standardIdentifier;
        }

        /// <summary>
        /// Receive Buffer Number. Must be a value of 0 - 1.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// SID[10:3]: Standard Identifier bits.
        /// These bits contain the eight Most Significant bits of the Standard Identifier for the received message.
        /// </summary>
        public byte StandardIdentifier { get; }

        private Address GetAddress() => RxBufferNumber switch
        {
            0 => Address.RxB0Sidh,
            1 => Address.RxB1Sidh,
            _ => throw new Exception($"Invalid value for {nameof(RxBufferNumber)}: {RxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to up look Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static byte GetRxBufferNumber(Address address) => address switch
        {
            Address.RxB0Sidh => 0,
            Address.RxB1Sidh => 1,
            _ => throw new ArgumentException($"Invalid value: {address}.", nameof(address)),
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
        public byte ToByte() => StandardIdentifier;
    }
}
