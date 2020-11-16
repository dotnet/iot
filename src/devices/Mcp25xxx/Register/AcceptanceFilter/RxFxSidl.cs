// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
                throw new ArgumentException(nameof(rxFilterNumber), $"Invalid RX Filter Number value {rxFilterNumber}.");
            }

            if (extendedIdentifierFilter > 3)
            {
                throw new ArgumentException(nameof(extendedIdentifierFilter), $"Invalid EID value {extendedIdentifierFilter}.");
            }

            if (standardIdentifierFilter > 7)
            {
                throw new ArgumentException(nameof(standardIdentifierFilter), $"Invalid SID value {standardIdentifierFilter}.");
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
                throw new ArgumentException(nameof(rxFilterNumber), $"Invalid RX Filter Number value {rxFilterNumber}.");
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

        private Address GetAddress() => RxFilterNumber switch
        {
            0 => Address.RxF0Sidl,
            1 => Address.RxF1Sidl,
            2 => Address.RxF2Sidl,
            3 => Address.RxF3Sidl,
            4 => Address.RxF4Sidl,
            5 => Address.RxF5Sidl,
            _ => throw new Exception($"Invalid value for {nameof(RxFilterNumber)}: {RxFilterNumber}."),
        };

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static byte GetRxFilterNumber(Address address) => address switch
        {
            Address.RxF0Sidl => 0,
            Address.RxF1Sidl => 1,
            Address.RxF2Sidl => 2,
            Address.RxF3Sidl => 3,
            Address.RxF4Sidl => 4,
            Address.RxF5Sidl => 5,
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
