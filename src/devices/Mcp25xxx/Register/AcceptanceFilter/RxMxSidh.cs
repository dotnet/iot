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

        private Address GetAddress() => RxMaskNumber switch
        {
            0 => Address.RxM0Sidh,
            1 => Address.RxM1Sidh,
            _ => throw new Exception($"Invalid value for {nameof(RxMaskNumber)}: {RxMaskNumber}."),
        };

        /// <summary>
        /// Gets the Rx Mask Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Mask Number.</param>
        /// <returns>The Rx Mask Number based on the register address.</returns>
        public static byte GetRxMaskNumber(Address address) => address switch
        {
            Address.RxM0Sidh => 0,
            Address.RxM1Sidh => 1,
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
        public byte ToByte() => StandardIdentifierMask;
    }
}
