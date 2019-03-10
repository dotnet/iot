// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="eid">Extended Identifier Mask bits.</param>
        /// <param name="sid">Standard Identifier Mask bits.</param>
        public RxMxSidl(RxMaskNumber rxMaskNumber, byte eid, byte sid)
        {
            if (eid > 3)
            {
                throw new ArgumentException($"Invalid EID value {eid}.", nameof(eid));
            }

            if (sid > 7)
            {
                throw new ArgumentException($"Invalid SID value {sid}.", nameof(sid));
            }

            RxMaskNumber = rxMaskNumber;
            Eid = eid;
            Sid = sid;
        }

        /// <summary>
        /// Initializes a new instance of the RxMxSidl class.
        /// </summary>
        /// <param name="rxMaskNumber">Receive Mask Number.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public RxMxSidl(RxMaskNumber rxMaskNumber, byte value)
        {
            RxMaskNumber = rxMaskNumber;
            Eid = (byte)(value & 0b0000_0011);
            Sid = (byte)((value & 0b1100_0000) >> 5);
        }

        /// <summary>
        /// Receive Mask Number.
        /// </summary>
        public RxMaskNumber RxMaskNumber { get; }

        /// <summary>
        /// Extended Identifier Mask bits.
        /// </summary>
        public byte Eid { get; }

        /// <summary>
        /// Standard Identifier Mask bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (RxMaskNumber)
            {
                case RxMaskNumber.Zero:
                    return Address.RxM0Sidl;
                case RxMaskNumber.One:
                    return Address.RxM1Sidl;
                default:
                    throw new ArgumentException("Invalid Rx Mask Number.", nameof(RxMaskNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Mask Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Mask Number.</param>
        /// <returns>The Rx Mask Number based on the register address.</returns>
        public static RxMaskNumber GetRxMaskNumber(Address address)
        {
            switch (address)
            {
                case Address.RxM0Sidl:
                    return RxMaskNumber.Zero;
                case Address.RxM1Sidl:
                    return RxMaskNumber.One;
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
            byte value = (byte)(Sid << 5);
            value |= Eid;
            return value;
        }
    }
}
