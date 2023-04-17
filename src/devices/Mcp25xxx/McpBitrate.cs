// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Bit Timing Configuration Registers
    /// </summary>
    public static class McpBitrate
    {
        private static readonly Dictionary<FrequencyAndSpeed, Tuple<byte, byte, byte>> s_bitTimingConfiguration = new()
        {
            { FrequencyAndSpeed._8MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0x80, 0x80) },
            { FrequencyAndSpeed._8MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0x90, 0x82) },
            { FrequencyAndSpeed._8MHz250KBps, new Tuple<byte, byte, byte>(0x00, 0xB1, 0x85) },
            { FrequencyAndSpeed._8MHz200KBps, new Tuple<byte, byte, byte>(0x00, 0xB4, 0x86) },
            { FrequencyAndSpeed._8MHz125KBps, new Tuple<byte, byte, byte>(0x01, 0xB1, 0x85) },
            { FrequencyAndSpeed._8MHz100KBps, new Tuple<byte, byte, byte>(0x01, 0xB4, 0x86) },
            { FrequencyAndSpeed._8MHz80KBps, new Tuple<byte, byte, byte>(0x01, 0xBF, 0x87) },
            { FrequencyAndSpeed._8MHz50KBps, new Tuple<byte, byte, byte>(0x03, 0xB4, 0x86) },
            { FrequencyAndSpeed._8MHz40KBps, new Tuple<byte, byte, byte>(0x03, 0xBF, 0x87) },
            { FrequencyAndSpeed._8MHz20KBps, new Tuple<byte, byte, byte>(0x07, 0xBF, 0x87) },
            { FrequencyAndSpeed._8MHz10KBps, new Tuple<byte, byte, byte>(0x0F, 0xBF, 0x87) },
            { FrequencyAndSpeed._8MHz5KBps, new Tuple<byte, byte, byte>(0x1F, 0xBF, 0x87) },
            { FrequencyAndSpeed._12MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0x88, 0x01) },
            { FrequencyAndSpeed._12MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0x9B, 0x02) },
            { FrequencyAndSpeed._12MHz250KBps, new Tuple<byte, byte, byte>(0x00, 0xBF, 0x06) },
            { FrequencyAndSpeed._12MHz200KBps, new Tuple<byte, byte, byte>(0x01, 0xA4, 0x03) },
            { FrequencyAndSpeed._12MHz125KBps, new Tuple<byte, byte, byte>(0x01, 0xBF, 0x06) },
            { FrequencyAndSpeed._12MHz100KBps, new Tuple<byte, byte, byte>(0x02, 0xB6, 0x04) },
            { FrequencyAndSpeed._12MHz80KBps, new Tuple<byte, byte, byte>(0x04, 0xA4, 0x03) },
            { FrequencyAndSpeed._12MHz50KBps, new Tuple<byte, byte, byte>(0x05, 0xB6, 0x04) },
            { FrequencyAndSpeed._12MHz40KBps, new Tuple<byte, byte, byte>(0x09, 0xA4, 0x03) },
            { FrequencyAndSpeed._12MHz20KBps, new Tuple<byte, byte, byte>(0x0C, 0xBF, 0x05) },
            { FrequencyAndSpeed._16MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0xD0, 0x82) },
            { FrequencyAndSpeed._16MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0xF0, 0x86) },
            { FrequencyAndSpeed._16MHz250KBps, new Tuple<byte, byte, byte>(0x41, 0xF1, 0x85) },
            { FrequencyAndSpeed._16MHz200KBps, new Tuple<byte, byte, byte>(0x01, 0xFA, 0x87) },
            { FrequencyAndSpeed._16MHz125KBps, new Tuple<byte, byte, byte>(0x03, 0xF0, 0x86) },
            { FrequencyAndSpeed._16MHz100KBps, new Tuple<byte, byte, byte>(0x03, 0xFA, 0x87) },
            { FrequencyAndSpeed._16MHz80KBps, new Tuple<byte, byte, byte>(0x03, 0xFF, 0x87) },
            { FrequencyAndSpeed._16MHz50KBps, new Tuple<byte, byte, byte>(0x07, 0xFA, 0x87) },
            { FrequencyAndSpeed._16MHz40KBps, new Tuple<byte, byte, byte>(0x07, 0xFF, 0x87) },
            { FrequencyAndSpeed._16MHz20KBps, new Tuple<byte, byte, byte>(0x0F, 0xFF, 0x87) },
            { FrequencyAndSpeed._16MHz10KBps, new Tuple<byte, byte, byte>(0x1F, 0xFF, 0x87) },
            { FrequencyAndSpeed._16MHz5KBps, new Tuple<byte, byte, byte>(0x3F, 0xFF, 0x87) },
            { FrequencyAndSpeed._20MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0xD9, 0x82) },
            { FrequencyAndSpeed._20MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0xFA, 0x87) },
            { FrequencyAndSpeed._20MHz250KBps, new Tuple<byte, byte, byte>(0x41, 0xFB, 0x86) },
            { FrequencyAndSpeed._20MHz200KBps, new Tuple<byte, byte, byte>(0x01, 0xFF, 0x87) },
            { FrequencyAndSpeed._20MHz125KBps, new Tuple<byte, byte, byte>(0x03, 0xFA, 0x87) },
            { FrequencyAndSpeed._20MHz100KBps, new Tuple<byte, byte, byte>(0x04, 0xFA, 0x87) },
            { FrequencyAndSpeed._20MHz80KBps, new Tuple<byte, byte, byte>(0x04, 0xFF, 0x87) },
            { FrequencyAndSpeed._20MHz50KBps, new Tuple<byte, byte, byte>(0x09, 0xFA, 0x87) },
            { FrequencyAndSpeed._20MHz40KBps, new Tuple<byte, byte, byte>(0x09, 0xFF, 0x87) }
        };

        /// <summary>
        /// Get bit timing configuration for specific CAN Bus frequency and speed
        /// </summary>
        /// <param name="frequencyAndSpeed">One of CAN Bus frequency and speed</param>
        /// <returns>The configuration for registers (CNF1, CNF2, CNF3)</returns>
        public static Tuple<byte, byte, byte> GetBitTimingConfiguration(FrequencyAndSpeed frequencyAndSpeed)
        {
            return s_bitTimingConfiguration[frequencyAndSpeed];
        }
    }
}
