// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="rxMaskNumber">Receive Mask Number.  Must be a value of 0 - 1.</param>
        /// <param name="extendedIdentifier">
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold the filter bits to be applied to bits[15:8] of the Extended Identifier portion of a received
        /// message.If corresponding with RXM[1:0] = 00 and EXIDE = 0, these bits are applied to Byte 0 in received data.
        /// </param>
        public RxMxEid8(byte rxMaskNumber, byte extendedIdentifier)
        {
            if (rxMaskNumber > 1)
            {
                throw new ArgumentException(nameof(rxMaskNumber), $"Invalid RX Mask Number value {rxMaskNumber}.");
            }

            RxMaskNumber = rxMaskNumber;
            ExtendedIdentifier = extendedIdentifier;
        }

        /// <summary>
        /// Receive Mask Number.  Must be a value of 0 - 1.
        /// </summary>
        public byte RxMaskNumber { get; }

        /// <summary>
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold the filter bits to be applied to bits[15:8] of the Extended Identifier portion of a received
        /// message.If corresponding with RXM[1:0] = 00 and EXIDE = 0, these bits are applied to Byte 0 in received data.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        private Address GetAddress() => RxMaskNumber switch
        {
            0 => Address.RxM0Eid8,
            1 => Address.RxM1Eid8,
            _ => throw new Exception($"Invalid value for {nameof(RxMaskNumber)}: {RxMaskNumber}."),
        };

        /// <summary>
        /// Gets the Rx Mask Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Mask Number.</param>
        /// <returns>The Rx Mask Number based on the register address.</returns>
        public static byte GetRxMaskNumber(Address address) => address switch
        {
            Address.RxM0Eid8 => 0,
            Address.RxM1Eid8 => 1,
            _ => throw new ArgumentException(nameof(address), $"Invalid value: {address}."),
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
