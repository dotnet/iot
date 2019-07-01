// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Standard Identifier Low Register.
    /// </summary>
    public class RxFxSidl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxFxSidl class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number.  Must be a value of 0 - 5.</param>
        /// <param name="extendedIdentifierFilter">
        /// EID[17:16]: Extended Identifier Filter bits.
        /// These bits hold the filter bits to be applied to bits[17:16] of the Extended Identifier portion of a received message.
        /// </param>
        /// <param name="extendedIdentifierEnable">
        /// EXIDE: Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </param>
        /// <param name="standardIdentifierFilter">
        /// SID[2:0]: Standard Identifier Filter bits.
        /// These bits hold the filter bits to be applied to bits[2:0] of the Standard Identifier portion of a received message.
        /// </param>
        public RxFxSidl(byte rxFilterNumber, byte extendedIdentifierFilter, bool extendedIdentifierEnable, byte standardIdentifierFilter)
        {
            if (rxFilterNumber > 5)
            {
                throw new ArgumentException($"Invalid RX Filter Number value {rxFilterNumber}.", nameof(rxFilterNumber));
            }

            if (extendedIdentifierFilter > 3)
            {
                throw new ArgumentException($"Invalid EID value {extendedIdentifierFilter}.", nameof(extendedIdentifierFilter));
            }

            if (standardIdentifierFilter > 7)
            {
                throw new ArgumentException($"Invalid SID value {standardIdentifierFilter}.", nameof(standardIdentifierFilter));
            }

            RxFilterNumber = rxFilterNumber;
            ExtendedIdentifierFilter = extendedIdentifierFilter;
            ExtendedIdentifierEnable = extendedIdentifierEnable;
            StandardIdentifierFilter = standardIdentifierFilter;
        }

        /// <summary>
        /// Initializes a new instance of the RxFxSidl class.
        /// </summary>
        /// <param name="rxFilterNumber">Receive Filter Number. Must be a value of 0 - 5.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public RxFxSidl(byte rxFilterNumber, byte value)
        {
            if (rxFilterNumber > 5)
            {
                throw new ArgumentException($"Invalid RX Filter Number value {rxFilterNumber}.", nameof(rxFilterNumber));
            }

            RxFilterNumber = rxFilterNumber;
            ExtendedIdentifierFilter = (byte)(value & 0b0000_0011);
            ExtendedIdentifierEnable = (value & 0b0000_1000) == 0b0000_1000;
            StandardIdentifierFilter = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Receive Filter Number.  Must be a value of 0 - 5.
        /// </summary>
        public byte RxFilterNumber { get; }

        /// <summary>
        /// EID[17:16]: Extended Identifier Filter bits.
        /// These bits hold the filter bits to be applied to bits[17:16] of the Extended Identifier portion of a received message.
        /// </summary>
        public byte ExtendedIdentifierFilter { get; }

        /// <summary>
        /// EXIDE: Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </summary>
        public bool ExtendedIdentifierEnable { get; }

        /// <summary>
        /// SID[2:0]:Standard Identifier Filter bits.
        /// These bits hold the filter bits to be applied to bits[2:0] of the Standard Identifier portion of a received message.
        /// </summary>
        public byte StandardIdentifierFilter { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case 0:
                    return Address.RxF0Sidl;
                case 1:
                    return Address.RxF1Sidl;
                case 2:
                    return Address.RxF2Sidl;
                case 3:
                    return Address.RxF3Sidl;
                case 4:
                    return Address.RxF4Sidl;
                case 5:
                    return Address.RxF5Sidl;
                default:
                    throw new ArgumentException($"Invalid Rx Filter Number value {RxFilterNumber}.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static byte GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Sidl:
                    return 0;
                case Address.RxF1Sidl:
                    return 1;
                case Address.RxF2Sidl:
                    return 2;
                case Address.RxF3Sidl:
                    return 3;
                case Address.RxF4Sidl:
                    return 4;
                case Address.RxF5Sidl:
                    return 5;
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
            byte value = (byte)(StandardIdentifierFilter << 5);

            if (ExtendedIdentifierEnable)
            {
                value |= 0b0000_1000;
            }

            value |= ExtendedIdentifierFilter;
            return value;
        }
    }
}
