// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Data Byte Register.
    /// </summary>
    public class RxBxDn : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxBxDn class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="index">Index of data.  Must be a value of 0 - 7.</param>
        /// <param name="receiveBufferDataFieldBytes">RBxD[7:0]: Receive Buffer Data Field Bytes.</param>
        public RxBxDn(byte rxBufferNumber, byte index, byte receiveBufferDataFieldBytes)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            if (index > 7)
            {
                throw new ArgumentException($"Invalid Index value {index}.", nameof(index));
            }

            RxBufferNumber = rxBufferNumber;
            Index = index;
            ReceiveBufferDataFieldBytes = receiveBufferDataFieldBytes;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// Index of data.  Must be a value of 0 - 7.
        /// </summary>
        public byte Index { get; }

        /// <summary>
        /// RBxD[7:0]: Receive Buffer Data Field Bytes.
        /// </summary>
        public byte ReceiveBufferDataFieldBytes { get; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case 0:
                    return (Address)((byte)Address.RxB0D0 + Index);
                case 1:
                    return (Address)((byte)Address.RxB1D0 + Index);
                default:
                    throw new ArgumentException($"Invalid Rx Bufferer Number value {RxBufferNumber}.", nameof(RxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static byte GetRxBufferNumber(Address address)
        {
            if (address >= Address.RxB0D0 && address <= Address.RxB0D7)
            {
                return 0;
            }
            else if (address >= Address.RxB1D0 && address <= Address.RxB1D7)
            {
                return 1;
            }

            throw new ArgumentException($"Invalid address value {address}.", nameof(address));
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
        public byte ToByte() => ReceiveBufferDataFieldBytes;
    }
}
