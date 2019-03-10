// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.BitTimeConfiguration
{
    /// <summary>
    /// Configuration 2 Register.
    /// </summary>
    public class Cnf2 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Cnf2 class.
        /// </summary>
        /// <param name="prseg">Propagation Segment Length bits.</param>
        /// <param name="phseg1">PS1 Length bits.</param>
        /// <param name="sam">
        /// Sample Point Configuration bit.
        /// True = Bus line is sampled three times at the sample point.
        /// False = Bus line is sampled once at the sample point.
        /// </param>
        /// <param name="btlmode">
        /// PS2 Bit Time Length bit.
        /// True = Length of PS2 is determined by the PHSEG2[2:0] bits of CNF3.
        /// False = Length of PS2 is the greater of PS1 and IPT(2 TQ).
        /// </param>
        public Cnf2(byte prseg, byte phseg1, bool sam, bool btlmode)
        {
            if (prseg > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PRSEG value {prseg}.", nameof(prseg));
            }

            if (phseg1 > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PHSEG1 value {phseg1}.", nameof(phseg1));
            }

            PrSeg = prseg;
            PhSeg1 = phseg1;
            Sam = sam;
            BtlMode = btlmode;
        }

        /// <summary>
        /// Initializes a new instance of the Cnf2 class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public Cnf2(byte value)
        {
            PrSeg = (byte)(value & 0b0000_0111);
            PhSeg1 = (byte)((value & 0b0011_1000) >> 3);
            Sam = (value & 0b0100_0000) == 0b0100_0000;
            BtlMode = (value & 0b1000_0000) == 0b1000_0000;
        }

        /// <summary>
        /// Propagation Segment Length bits.
        /// </summary>
        public byte PrSeg { get; }

        /// <summary>
        /// PS1 Length bits.
        /// </summary>
        public byte PhSeg1 { get; }

        /// <summary>
        /// Sample Point Configuration bit.
        /// True = Bus line is sampled three times at the sample point.
        /// False = Bus line is sampled once at the sample point.
        /// </summary>
        public bool Sam { get; }

        /// <summary>
        /// PS2 Bit Time Length bit.
        /// True = Length of PS2 is determined by the PHSEG2[2:0] bits of CNF3.
        /// False = Length of PS2 is the greater of PS1 and IPT(2 TQ).
        /// </summary>
        public bool BtlMode { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Cnf2;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (BtlMode)
            {
                value |= 0b1000_0000;
            }

            if (Sam)
            {
                value |= 0b0100_0000;
            }

            value |= (byte)(PhSeg1 << 3);
            value |= PrSeg;
            return value;
        }
    }
}
