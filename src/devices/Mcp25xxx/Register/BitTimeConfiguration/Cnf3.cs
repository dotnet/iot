// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="ps2Length">
        /// PHSEG2[2:0]: PS2 Length bits.
        /// (PHSEG2[2:0] + 1) x TQ.
        /// Minimum valid setting for PS2 is 2 TQ.
        /// </param>
        /// <param name="wakeUpFilter">
        /// WAKFIL: Wake-up Filter bit.
        /// True = Wake-up filter is enabled.
        /// False = Wake-up filter is disabled.
        /// </param>
        /// <param name="startOfFrameSignal">
        /// SOF: Start-of-Frame Signal bit.
        /// If CLKEN(CANCTRL[2]) = 1:
        /// True = CLKOUT pin is enabled for SOF signal.
        /// False = CLKOUT pin is enabled for clock out function;
        /// If CLKEN(CANCTRL[2]) = 0:
        /// Bit is don’t care.
        /// </param>
        public Cnf3(byte ps2Length, bool wakeUpFilter, bool startOfFrameSignal)
        {
            if (ps2Length > 0b0000_0111)
            {
                throw new ArgumentException($"Invalid PHSEG2 value {ps2Length}.", nameof(ps2Length));
            }

            Ps2Length = ps2Length;
            WakeUpFilter = wakeUpFilter;
            StartOfFrameSignal = startOfFrameSignal;
        }

        /// <summary>
        /// Initializes a new instance of the Cnf3 class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public Cnf3(byte value)
        {
            Ps2Length = (byte)(value & 0b0000_0111);
            WakeUpFilter = (value & 0b0100_0000) == 0b0100_0000;
            StartOfFrameSignal = (value & 0b1000_0000) == 0b1000_0000;
        }

        /// <summary>
        /// PHSEG2[2:0]: PS2 Length bits.
        /// (PHSEG2[2:0] + 1) x TQ.
        /// Minimum valid setting for PS2 is 2 TQ.
        /// </summary>
        public byte Ps2Length { get; }

        /// <summary>
        /// WAKFIL: Wake-up Filter bit.
        /// True = Wake-up filter is enabled.
        /// False = Wake-up filter is disabled.
        /// </summary>
        public bool WakeUpFilter { get; }

        /// <summary>
        /// SOF: Start-of-Frame Signal bit.
        /// If CLKEN(CANCTRL[2]) = 1:
        /// True = CLKOUT pin is enabled for SOF signal.
        /// False = CLKOUT pin is enabled for clock out function;
        /// If CLKEN(CANCTRL[2]) = 0:
        /// Bit is don’t care.
        /// </summary>
        public bool StartOfFrameSignal { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Cnf3;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (StartOfFrameSignal)
            {
                value |= 0b1000_0000;
            }

            if (WakeUpFilter)
            {
                value |= 0b0100_0000;
            }

            value |= Ps2Length;
            return value;
        }
    }
}
