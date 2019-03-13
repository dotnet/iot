// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Standard Identifier Low Register.
    /// </summary>
    public class RxBxSidl : IRegister
    {
        /// <summary>
        ///  Initializes a new instance of the RxBxSidl class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="eid">
        /// Extended Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </param>
        /// <param name="ide">
        /// Extended Identifier Flag bit.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </param>
        /// <param name="srr">
        /// Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </param>
        /// <param name="sid">
        /// Extended Identifier bits.
        /// </param>
        public RxBxSidl(byte rxBufferNumber, byte eid, bool ide, bool srr, byte sid)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            if (eid > 3)
            {
                throw new ArgumentException($"Invalid EID value {eid}.", nameof(eid));
            }

            if (sid > 7)
            {
                throw new ArgumentException($"Invalid SID value {sid}.", nameof(sid));
            }

            RxBufferNumber = rxBufferNumber;
            Eid = eid;
            Ide = ide;
            Srr = srr;
            Sid = sid;
        }

        /// <summary>
        /// Initializes a new instance of the RxBxSidl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxBxSidl(byte rxBufferNumber, byte value)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            Eid = (byte)(value & 0b0000_0011);
            Ide = ((value >> 3) & 1) == 1;
            Srr = ((value >> 4) & 1) == 1;
            Sid = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Receive Buffer Number. Must be a value of 0 - 1.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </summary>
        public byte Eid { get; }

        /// <summary>
        /// Extended Identifier Flag bit.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </summary>
        public bool Ide { get; }

        /// <summary>
        /// Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </summary>
        public bool Srr { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case 0:
                    return Address.RxB0Sidl;
                case 1:
                    return Address.RxB1Sidl;
                default:
                    throw new ArgumentException($"Invalid Rx Buffer Number value {RxBufferNumber}.", nameof(RxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static byte GetRxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.RxB0Sidl:
                    return 0;
                case Address.RxB1Sidl:
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
            byte value = (byte)(Sid << 5);

            if (Srr)
            {
                value |= 0b0001_0000;
            }

            if (Ide)
            {
                value |= 0b0000_1000;
            }

            value |= Eid;
            return value;
        }
    }
}
