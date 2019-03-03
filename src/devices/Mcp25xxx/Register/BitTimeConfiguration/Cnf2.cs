// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.BitTimeConfiguration
{
    /// <summary>
    /// Configuration 2 Register.
    /// </summary>
    public class Cnf2
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
        public Cnf2(byte prseg, bool phseg1, bool sam, bool btlmode)
        {
            PrSeg = prseg;
            PhSeg1 = phseg1;
            Sam = sam;
            BtlMode = btlmode;
        }

        /// <summary>
        /// Propagation Segment Length bits.
        /// </summary>
        public byte PrSeg { get; set; }

        /// <summary>
        /// PS1 Length bits.
        /// </summary>
        public bool PhSeg1 { get; set; }

        /// <summary>
        /// Sample Point Configuration bit.
        /// True = Bus line is sampled three times at the sample point.
        /// False = Bus line is sampled once at the sample point.
        /// </summary>
        public bool Sam { get; set; }

        /// <summary>
        /// PS2 Bit Time Length bit.
        /// True = Length of PS2 is determined by the PHSEG2[2:0] bits of CNF3.
        /// False = Length of PS2 is the greater of PS1 and IPT(2 TQ).
        /// </summary>
        public bool BtlMode { get; set; }
    }
}
