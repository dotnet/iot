// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.AcceptanceFilter
{
    /// <summary>
    /// Filter Standard Identifier High Register.
    /// </summary>
    public class RxFxSidh : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxFxSidh class.
        /// </summary>
        /// <param name="rxFilterNumber">
        /// Receive Filter Number.  Must be a value of 0 - 5.</param>
        /// <param name="standardIdentifierFilter">
        /// SID[10:3]: Standard Identifier Filter bits
        /// These bits hold the filter bits to be applied to bits[10:3] of the Standard Identifier portion of a received message.
        /// </param>
        public RxFxSidh(byte rxFilterNumber, byte standardIdentifierFilter)
        {
            if (rxFilterNumber > 5)
            {
                throw new ArgumentException(nameof(rxFilterNumber), $"Invalid RX Filter Number value {rxFilterNumber}.");
            }

            RxFilterNumber = rxFilterNumber;
            StandardIdentifierFilter = standardIdentifierFilter;
        }

        /// <summary>
        /// Receive Filter Number.  Must be a value of 0 - 5.
        /// </summary>
        public byte RxFilterNumber { get; }

        /// <summary>
        /// SID[10:3]: Standard Identifier Filter bits
        /// These bits hold the filter bits to be applied to bits[10:3] of the Standard Identifier portion of a received message.
        /// </summary>
        public byte StandardIdentifierFilter { get; }

        private Address GetAddress() => RxFilterNumber switch
        {
            0 => Address.RxF0Sidh,
            1 => Address.RxF1Sidh,
            2 => Address.RxF2Sidh,
            3 => Address.RxF3Sidh,
            4 => Address.RxF4Sidh,
            5 => Address.RxF5Sidh,
            _ => throw new Exception($"Invalid value for {nameof(RxFilterNumber)}: {RxFilterNumber}."),
        };

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to up look Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static byte GetRxFilterNumber(Address address) => address switch
        {
            Address.RxF0Sidh => 0,
            Address.RxF1Sidh => 1,
            Address.RxF2Sidh => 2,
            Address.RxF3Sidh => 3,
            Address.RxF4Sidh => 4,
            Address.RxF5Sidh => 5,
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
        public byte ToByte() => StandardIdentifierFilter;
    }
}
