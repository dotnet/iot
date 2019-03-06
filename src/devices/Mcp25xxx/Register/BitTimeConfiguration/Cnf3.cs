// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.BitTimeConfiguration
{
    /// <summary>
    /// Configuration 3 Register.
    /// </summary>
    public class Cnf3 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Cnf3 class.
        /// </summary>
        /// <param name="phseg2">
        /// PS2 Length bits.
        /// (PHSEG2[2:0] + 1) x TQ.
        /// Minimum valid setting for PS2 is 2 TQ.
        /// </param>
        /// <param name="wakfil">
        /// Wake-up Filter bit.
        /// True = Wake-up filter is enabled.
        /// False = Wake-up filter is disabled.
        /// </param>
        /// <param name="sof">
        /// Start-of-Frame Signal bit.
        /// If CLKEN(CANCTRL[2]) = 1:
        /// True = CLKOUT pin is enabled for SOF signal.
        /// False = CLKOUT pin is enabled for clock out function;
        /// If CLKEN(CANCTRL[2]) = 0:
        /// Bit is don’t care.
        /// </param>
        public Cnf3(byte phseg2, bool wakfil, bool sof)
        {
            if (phseg2 > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PHSEG2 value {phseg2}.", nameof(phseg2));
            }

            PhSeg2 = phseg2;
            WakFil = wakfil;
            Sof = sof;
        }

        /// <summary>
        /// PS2 Length bits.
        /// (PHSEG2[2:0] + 1) x TQ.
        /// Minimum valid setting for PS2 is 2 TQ.
        /// </summary>
        public byte PhSeg2 { get; set; }

        /// <summary>
        /// Wake-up Filter bit.
        /// True = Wake-up filter is enabled.
        /// False = Wake-up filter is disabled.
        /// </summary>
        public bool WakFil { get; set; }

        /// <summary>
        /// Start-of-Frame Signal bit.
        /// If CLKEN(CANCTRL[2]) = 1:
        /// True = CLKOUT pin is enabled for SOF signal.
        /// False = CLKOUT pin is enabled for clock out function;
        /// If CLKEN(CANCTRL[2]) = 0:
        /// Bit is don’t care.
        /// </summary>
        public bool Sof { get; set; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address GetAddress() => Address.Cnf3;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (Sof)
            {
                value |= 0b1000_0000;
            }

            if (WakFil)
            {
                value |= 0b0100_0000;
            }

            value |= PhSeg2;
            return value;
        }
    }
}
