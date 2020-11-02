// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Extended Identifier Low Register.
    /// </summary>
    public class RxMxEid0 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxMxEid0 class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.  Must be a value of 0 - 1.</param>
        /// <param name="extendedIdentifierMask">
        /// EID[7:0]: Extended Identifier Mask bits.
        /// These bits hold the filter bits to be applied to bits[7:0] of the Extended Identifier portion of a received
        /// message.If corresponding with RXM[1:0] = 00 and EXIDE = 0, these bits are applied to Byte 1 in received data.
        /// </param>
        public RxMxEid0(byte rxMaskNumber, byte extendedIdentifierMask)
        {
            if (rxMaskNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Mask Number value {rxMaskNumber}.", nameof(rxMaskNumber));
            }

            RxMaskNumber = rxMaskNumber;
            ExtendedIdentifierMask = extendedIdentifierMask;
        }

        /// <summary>
        /// Receive Mask Number.  Must be a value of 0 - 1.
        /// </summary>
        public byte RxMaskNumber { get; }

        /// <summary>
        /// EID[7:0]: Extended Identifier Mask bits.
        /// These bits hold the filter bits to be applied to bits[7:0] of the Extended Identifier portion of a received
        /// message.If corresponding with RXM[1:0] = 00 and EXIDE = 0, these bits are applied to Byte 1 in received data.
        /// </summary>
        public byte ExtendedIdentifierMask { get; }

        private Address GetAddress()
        {
            switch (RxMaskNumber)
            {
                case 0:
                    return Address.RxM0Eid0;
                case 1:
                    return Address.RxM1Eid0;
                default:
                    throw new ArgumentException($"Invalid Rx Mask Number value {RxMaskNumber}.", nameof(RxMaskNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Mask Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Mask Number.</param>
        /// <returns>The Rx Mask Number based on the register address.</returns>
        public static byte GetRxMaskNumber(Address address)
        {
            switch (address)
            {
                case Address.RxM0Eid0:
                    return 0;
                case Address.RxM1Eid0:
                    return 1;
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
        public byte ToByte() => ExtendedIdentifierMask;
    }
}
