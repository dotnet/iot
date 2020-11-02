// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="propagationSegmentLength">PRSEG[2:0]: Propagation Segment Length bits.</param>
        /// <param name="ps1Length">PHSEG1[2:0]: PS1 Length bits.</param>
        /// <param name="samplePointConfiguration">
        /// SAM: Sample Point Configuration bit.
        /// True = Bus line is sampled three times at the sample point.
        /// False = Bus line is sampled once at the sample point.
        /// </param>
        /// <param name="ps2BitTimeLength">
        /// BTLMODE: PS2 Bit Time Length bit.
        /// True = Length of PS2 is determined by the PHSEG2[2:0] bits of CNF3.
        /// False = Length of PS2 is the greater of PS1 and IPT(2 TQ).
        /// </param>
        public Cnf2(byte propagationSegmentLength, byte ps1Length, bool samplePointConfiguration, bool ps2BitTimeLength)
        {
            if (propagationSegmentLength > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PRSEG value {propagationSegmentLength}.", nameof(propagationSegmentLength));
            }

            if (ps1Length > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PHSEG1 value {ps1Length}.", nameof(ps1Length));
            }

            PropagationSegmentLength = propagationSegmentLength;
            Ps1Length = ps1Length;
            SamplePointConfiguration = samplePointConfiguration;
            Ps2BitTimeLength = ps2BitTimeLength;
        }

        /// <summary>
        /// Initializes a new instance of the Cnf2 class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public Cnf2(byte value)
        {
            PropagationSegmentLength = (byte)(value & 0b0000_0111);
            Ps1Length = (byte)((value & 0b0011_1000) >> 3);
            SamplePointConfiguration = (value & 0b0100_0000) == 0b0100_0000;
            Ps2BitTimeLength = (value & 0b1000_0000) == 0b1000_0000;
        }

        /// <summary>
        /// PRSEG[2:0]: Propagation Segment Length bits.
        /// </summary>
        public byte PropagationSegmentLength { get; }

        /// <summary>
        /// PHSEG1[2:0]: PS1 Length bits.
        /// </summary>
        public byte Ps1Length { get; }

        /// <summary>
        /// SAM: Sample Point Configuration bit.
        /// True = Bus line is sampled three times at the sample point.
        /// False = Bus line is sampled once at the sample point.
        /// </summary>
        public bool SamplePointConfiguration { get; }

        /// <summary>
        /// BTLMODE: PS2 Bit Time Length bit.
        /// True = Length of PS2 is determined by the PHSEG2[2:0] bits of CNF3.
        /// False = Length of PS2 is the greater of PS1 and IPT(2 TQ).
        /// </summary>
        public bool Ps2BitTimeLength { get; }

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

            if (Ps2BitTimeLength)
            {
                value |= 0b1000_0000;
            }

            if (SamplePointConfiguration)
            {
                value |= 0b0100_0000;
            }

            value |= (byte)(Ps1Length << 3);
            value |= PropagationSegmentLength;
            return value;
        }
    }
}
