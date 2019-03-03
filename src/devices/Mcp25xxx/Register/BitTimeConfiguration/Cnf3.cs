// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.BitTimeConfiguration
{
    /// <summary>
    /// Configuration 3 Register.
    /// </summary>
    public class Cnf3
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
        public Cnf3(byte phseg2, byte wakfil, bool sof)
        {
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
        public byte WakFil { get; set; }

        /// <summary>
        /// Start-of-Frame Signal bit.
        /// If CLKEN(CANCTRL[2]) = 1:
        /// True = CLKOUT pin is enabled for SOF signal.
        /// False = CLKOUT pin is enabled for clock out function;
        /// If CLKEN(CANCTRL[2]) = 0:
        /// Bit is don’t care.
        /// </summary>
        public bool Sof { get; set; }
    }
}
