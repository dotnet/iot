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
        /// <param name="rxFilterNumber">Receive Filter Number.</param>
        /// <param name="eid">Extended Identifier Filter bits.</param>
        /// <param name="exide">
        /// Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </param>
        /// <param name="sid">Standard Identifier Filter bits.</param>
        public RxFxSidl(RxFilterNumber rxFilterNumber, byte eid, bool exide, byte sid)
        {
            if (eid > 3)
            {
                throw new ArgumentException($"Invalid EID value {eid}.", nameof(eid));
            }

            if (sid > 7)
            {
                throw new ArgumentException($"Invalid SID value {sid}.", nameof(sid));
            }

            RxFilterNumber = rxFilterNumber;
            Eid = eid;
            Exide = exide;
            Sid = sid;
        }

        /// <summary>
        /// Initializes a new instance of the RxFxSidl class.
        /// </summary>
        /// <param name="rxFilterNumber">Transmit Buffer Number.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public RxFxSidl(RxFilterNumber rxFilterNumber, byte value)
        {
            RxFilterNumber = rxFilterNumber;
            Eid = (byte)(value & 0b0000_0011);
            Sid = (byte)((value & 0b1100_0000) >> 5);
        }

        /// <summary>
        /// Receive Filter Number.
        /// </summary>
        public RxFilterNumber RxFilterNumber { get; }

        /// <summary>
        /// Extended Identifier Filter bits.
        /// </summary>
        public byte Eid { get; }

        /// <summary>
        /// Extended Identifier Enable bit.
        /// True = Filter is applied only to extended frames.
        /// False = Filter is applied only to standard frames.
        /// </summary>
        public bool Exide { get; }

        /// <summary>
        /// Standard Identifier Filter bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (RxFilterNumber)
            {
                case RxFilterNumber.Zero:
                    return Address.RxF0Sidl;
                case RxFilterNumber.One:
                    return Address.RxF1Sidl;
                case RxFilterNumber.Two:
                    return Address.RxF2Sidl;
                case RxFilterNumber.Three:
                    return Address.RxF3Sidl;
                case RxFilterNumber.Four:
                    return Address.RxF4Sidl;
                case RxFilterNumber.Five:
                    return Address.RxF5Sidl;
                default:
                    throw new ArgumentException("Invalid Rx Filter Number.", nameof(RxFilterNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Filter Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Filter Number.</param>
        /// <returns>The Rx Filter Number based on the register address.</returns>
        public static RxFilterNumber GetRxFilterNumber(Address address)
        {
            switch (address)
            {
                case Address.RxF0Sidl:
                    return RxFilterNumber.Zero;
                case Address.RxF1Sidl:
                    return RxFilterNumber.One;
                case Address.RxF2Sidl:
                    return RxFilterNumber.Two;
                case Address.RxF3Sidl:
                    return RxFilterNumber.Three;
                case Address.RxF4Sidl:
                    return RxFilterNumber.Four;
                case Address.RxF5Sidl:
                    return RxFilterNumber.Five;
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

            if (Exide)
            {
                value |= 0b0000_1000;
            }

            value |= Eid;
            return value;
        }
    }
}
