// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Mask Standard Identifier Low Register.
    /// </summary>
    public class RxMxSidl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxMxSidl class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.  Must be a value of 0 - 1.</param>
        /// <param name="extendedIdentifierMask">
        /// EID[17:16]: Extended Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[17:16] of the Extended Identifier portion of a received message.
        /// </param>
        /// <param name="standardIdentifierMask">
        /// SID[2:0]: Standard Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[2:0] of the Standard Identifier portion of a received message.
        /// </param>
        public RxMxSidl(byte rxMaskNumber, byte extendedIdentifierMask, byte standardIdentifierMask)
        {
            if (rxMaskNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Mask Number value {rxMaskNumber}.", nameof(rxMaskNumber));
            }

            if (extendedIdentifierMask > 3)
            {
                throw new ArgumentException($"Invalid EID value {extendedIdentifierMask}.", nameof(extendedIdentifierMask));
            }

            if (standardIdentifierMask > 7)
            {
                throw new ArgumentException($"Invalid SID value {standardIdentifierMask}.", nameof(standardIdentifierMask));
            }

            RxMaskNumber = rxMaskNumber;
            ExtendedIdentifierMask = extendedIdentifierMask;
            StandardIdentifierMask = standardIdentifierMask;
        }

        /// <summary>
        /// Initializes a new instance of the RxMxSidl class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.  Must be a value of 0 - 1.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public RxMxSidl(byte rxMaskNumber, byte value)
        {
            if (rxMaskNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Mask Number value {rxMaskNumber}.", nameof(rxMaskNumber));
            }

            RxMaskNumber = rxMaskNumber;
            ExtendedIdentifierMask = (byte)(value & 0b0000_0011);
            StandardIdentifierMask = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Receive Mask Number.  Must be a value of 0 - 1.
        /// </summary>
        public byte RxMaskNumber { get; }

        /// <summary>
        /// EID[17:16]: Extended Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[17:16] of the Extended Identifier portion of a received message.
        /// </summary>
        public byte ExtendedIdentifierMask { get; }

        /// <summary>
        /// SID[2:0]: Standard Identifier Mask bits.
        /// These bits hold the mask bits to be applied to bits[2:0] of the Standard Identifier portion of a received message.
        /// </summary>
        public byte StandardIdentifierMask { get; }

        private Address GetAddress()
        {
            switch (RxMaskNumber)
            {
                case 0:
                    return Address.RxM0Sidl;
                case 1:
                    return Address.RxM1Sidl;
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
                case Address.RxM0Sidl:
                    return 0;
                case Address.RxM1Sidl:
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
        public byte ToByte()
        {
            byte value = (byte)(StandardIdentifierMask << 5);
            value |= ExtendedIdentifierMask;
            return value;
        }
    }
}
