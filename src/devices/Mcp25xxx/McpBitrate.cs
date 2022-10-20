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
        /// <summary>
        /// Standard Frequency and Speed for CAN bus
        /// </summary>
        public enum CanBusFrequencyAndSpeed
        {
            /// <summary>
            /// 8MHz 1000kBPS
            /// </summary>
            _8MHz1000KBps,

            /// <summary>
            /// 8MHz 500kBPS
            /// </summary>
            _8MHz500KBps,

            /// <summary>
            /// 8MHz 250kBPS
            /// </summary>
            _8MHz250KBps,

            /// <summary>
            /// 8MHz 200kBPS
            /// </summary>
            _8MHz200KBps,

            /// <summary>
            /// 8MHz 125kBPS
            /// </summary>
            _8MHz125KBps,

            /// <summary>
            /// 8MHz 100kBPS
            /// </summary>
            _8MHz100KBps,

            /// <summary>
            /// 8MHz 80kBPS
            /// </summary>
            _8MHz80KBps,

            /// <summary>
            /// 8MHz 50kBPS
            /// </summary>
            _8MHz50KBps,

            /// <summary>
            /// 8MHz 40kBPS
            /// </summary>
            _8MHz40KBps,

            /// <summary>
            /// 8MHz 20kBPS
            /// </summary>
            _8MHz20KBps,

            /// <summary>
            /// 8MHz 10kBPS
            /// </summary>
            _8MHz10KBps,

            /// <summary>
            /// 8MHz 5kBPS
            /// </summary>
            _8MHz5KBps,

            /// <summary>
            /// 16MHz 1000kBPS
            /// </summary>
            _16MHz1000KBps,

            /// <summary>
            /// 16MHz 500kBPS
            /// </summary>
            _16MHz500KBps,

            /// <summary>
            /// 16MHz 250kBPS
            /// </summary>
            _16MHz250KBps,

            /// <summary>
            /// 16MHz 200kBPS
            /// </summary>
            _16MHz200KBps,

            /// <summary>
            /// 16MHz 125kBPS
            /// </summary>
            _16MHz125KBps,

            /// <summary>
            /// 16MHz 100kBPS
            /// </summary>
            _16MHz100KBps,

            /// <summary>
            /// 16MHz 80kBPS
            /// </summary>
            _16MHz80KBps,

            /// <summary>
            /// 16MHz 50kBPS
            /// </summary>
            _16MHz50KBps,

            /// <summary>
            /// 16MHz 40kBPS
            /// </summary>
            _16MHz40KBps,

            /// <summary>
            /// 16MHz 20kBPS
            /// </summary>
            _16MHz20KBps,

            /// <summary>
            /// 16MHz 10kBPS
            /// </summary>
            _16MHz10KBps,

            /// <summary>
            /// 16MHz 5kBPS
            /// </summary>
            _16MHz5KBps,

            /// <summary>
            /// 20MHz 1000kBPS
            /// </summary>
            _20MHz1000KBps,

            /// <summary>
            /// 20MHz 500kBPS
            /// </summary>
            _20MHz500KBps,

            /// <summary>
            /// 20MHz 250kBPS
            /// </summary>
            _20MHz250KBps,

            /// <summary>
            /// 20MHz 200kBPS
            /// </summary>
            _20MHz200KBps,

            /// <summary>
            /// 20MHz 125kBPS
            /// </summary>
            _20MHz125KBps,

            /// <summary>
            /// 20MHz 100kBPS
            /// </summary>
            _20MHz100KBps,

            /// <summary>
            /// 20MHz 80kBPS
            /// </summary>
            _20MHz80KBps,

            /// <summary>
            /// 20MHz 50kBPS
            /// </summary>
            _20MHz50KBps,

            /// <summary>
            /// 20MHz 40kBPS
            /// </summary>
            _20MHz40KBps,
        }

        private static Dictionary<CanBusFrequencyAndSpeed, Tuple<byte, byte, byte>> s_bitrates = new()
        {
            { CanBusFrequencyAndSpeed._8MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0x80, 0x80) },
            { CanBusFrequencyAndSpeed._8MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0x90, 0x82) },
            { CanBusFrequencyAndSpeed._8MHz250KBps, new Tuple<byte, byte, byte>(0x00, 0xB1, 0x85) },
            { CanBusFrequencyAndSpeed._8MHz200KBps, new Tuple<byte, byte, byte>(0x00, 0xB4, 0x86) },
            { CanBusFrequencyAndSpeed._8MHz125KBps, new Tuple<byte, byte, byte>(0x01, 0xB1, 0x85) },
            { CanBusFrequencyAndSpeed._8MHz100KBps, new Tuple<byte, byte, byte>(0x01, 0xB4, 0x86) },
            { CanBusFrequencyAndSpeed._8MHz80KBps, new Tuple<byte, byte, byte>(0x01, 0xBF, 0x87) },
            { CanBusFrequencyAndSpeed._8MHz50KBps, new Tuple<byte, byte, byte>(0x03, 0xB4, 0x86) },
            { CanBusFrequencyAndSpeed._8MHz40KBps, new Tuple<byte, byte, byte>(0x03, 0xBF, 0x87) },
            { CanBusFrequencyAndSpeed._8MHz20KBps, new Tuple<byte, byte, byte>(0x07, 0xBF, 0x87) },
            { CanBusFrequencyAndSpeed._8MHz10KBps, new Tuple<byte, byte, byte>(0x0F, 0xBF, 0x87) },
            { CanBusFrequencyAndSpeed._8MHz5KBps, new Tuple<byte, byte, byte>(0x1F, 0xBF, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0xD0, 0x82) },
            { CanBusFrequencyAndSpeed._16MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0xF0, 0x86) },
            { CanBusFrequencyAndSpeed._16MHz250KBps, new Tuple<byte, byte, byte>(0x41, 0xF1, 0x85) },
            { CanBusFrequencyAndSpeed._16MHz200KBps, new Tuple<byte, byte, byte>(0x01, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz125KBps, new Tuple<byte, byte, byte>(0x03, 0xF0, 0x86) },
            { CanBusFrequencyAndSpeed._16MHz100KBps, new Tuple<byte, byte, byte>(0x03, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz80KBps, new Tuple<byte, byte, byte>(0x03, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz50KBps, new Tuple<byte, byte, byte>(0x07, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz40KBps, new Tuple<byte, byte, byte>(0x07, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz20KBps, new Tuple<byte, byte, byte>(0x0F, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz10KBps, new Tuple<byte, byte, byte>(0x1F, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._16MHz5KBps, new Tuple<byte, byte, byte>(0x3F, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz1000KBps, new Tuple<byte, byte, byte>(0x00, 0xD9, 0x82) },
            { CanBusFrequencyAndSpeed._20MHz500KBps, new Tuple<byte, byte, byte>(0x00, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz250KBps, new Tuple<byte, byte, byte>(0x41, 0xFB, 0x86) },
            { CanBusFrequencyAndSpeed._20MHz200KBps, new Tuple<byte, byte, byte>(0x01, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz125KBps, new Tuple<byte, byte, byte>(0x03, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz100KBps, new Tuple<byte, byte, byte>(0x04, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz80KBps, new Tuple<byte, byte, byte>(0x04, 0xFF, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz50KBps, new Tuple<byte, byte, byte>(0x09, 0xFA, 0x87) },
            { CanBusFrequencyAndSpeed._20MHz40KBps, new Tuple<byte, byte, byte>(0x09, 0xFF, 0x87) }
        };

        /// <summary>
        /// Get bit timing configuration for specific CAN Bus frequency and speed
        /// </summary>
        /// <param name="frequencyAndSpeed">One of CAN Bus frequency and speed</param>
        /// <returns>The configuration for registers (CNF1, CNF2, CNF3)</returns>
        public static Tuple<byte, byte, byte> GetBitTimingConfiguration(CanBusFrequencyAndSpeed frequencyAndSpeed)
        {
            return s_bitrates[frequencyAndSpeed];
        }
    }
}
