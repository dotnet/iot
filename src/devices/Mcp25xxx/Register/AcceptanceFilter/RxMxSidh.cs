// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Standard Identifier High Register.
    /// </summary>
    public class RxMxSidh : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxMxSidh class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.  Must be a value of 0 - 1.</param>
        /// <param name="standardIdentifierMask">
        /// SID[10:3]: Standard Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[10:3] of the Standard Identifier portion of a received message.
        /// </param>
        public RxMxSidh(byte rxMaskNumber, byte standardIdentifierMask)
        {
            if (rxMaskNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Mask Number value {rxMaskNumber}.", nameof(rxMaskNumber));
            }

            RxMaskNumber = rxMaskNumber;
            StandardIdentifierMask = standardIdentifierMask;
        }

        /// <summary>
        /// Receive Mask Number.  Must be a value of 0 - 1.
        /// </summary>
        public byte RxMaskNumber { get; }

        /// <summary>
        /// SID[10:3]: Standard Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[10:3] of the Standard Identifier portion of a received message.
        /// </summary>
        public byte StandardIdentifierMask { get; }

        private Address GetAddress()
        {
            switch (RxMaskNumber)
            {
                case 0:
                    return Address.RxM0Sidh;
                case 1:
                    return Address.RxM1Sidh;
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
                case Address.RxM0Sidh:
                    return 0;
                case Address.RxM1Sidh:
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
        public byte ToByte() => StandardIdentifierMask;
    }
}
