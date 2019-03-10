// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Data Length Code Register.
    /// </summary>
    public class TxBxDlc : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxDlc class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="dlc">
        /// Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// </param>
        /// <param name="rtr">
        /// Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </param>
        public TxBxDlc(TxBufferNumber txBufferNumber, int dlc, bool rtr)
        {
            TxBufferNumber = txBufferNumber;
            Dlc = dlc;
            Rtr = rtr;
        }

        /// <summary>
        /// Initializes a new instance of the TxBxDlc class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public TxBxDlc(TxBufferNumber txBufferNumber, byte value)
        {
            TxBufferNumber = txBufferNumber;
            Dlc = (byte)(value & 0b0000_1111);
            Rtr = (value & 0b0100_0000) == 0b0100_0000;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; }

        /// <summary>
        /// Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// It is possible to set the DLC[3:0] bits to a value greater than eight; however, only eight bytes are transmitted.
        /// </summary>
        public int Dlc { get; }

        /// <summary>
        /// Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </summary>
        public bool Rtr { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case TxBufferNumber.Zero:
                    return Address.TxB0Dlc;
                case TxBufferNumber.One:
                    return Address.TxB1Dlc;
                case TxBufferNumber.Two:
                    return Address.TxB2Dlc;
                default:
                    throw new ArgumentException("Invalid Tx Buffer Number.", nameof(TxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static TxBufferNumber GetTxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.TxB0Dlc:
                    return TxBufferNumber.Zero;
                case Address.TxB1Dlc:
                    return TxBufferNumber.One;
                case Address.TxB2Dlc:
                    return TxBufferNumber.Two;
                default:
                    throw new ArgumentException("Invalid address.", nameof(address));
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
            byte value = 0;

            if (Rtr)
            {
                value |= 0b0100_0000;
            }

            value |= (byte)Dlc;
            return value;
        }
    }
}
