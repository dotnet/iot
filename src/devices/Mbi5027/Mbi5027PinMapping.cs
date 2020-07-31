// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents pin bindings for the MBI5027.
    /// </summary>
    public struct Mbi5027PinMapping
    {
        /// <param name="sdi">Serial data in pin</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="le">Register clock pin (latch)</param>
        /// <param name="clk">Shift register pin (shift to data register)</param>
        /// <param name="sdo">Serial data out pin -- required for open circuit detection</param>
        public Mbi5027PinMapping(int sdi, int oe, int le, int clk, int sdo = 0)
        {
            Sdi = sdi;          // data in;     SR pin 2
            OE = oe;            // blank;       SR pin 21
            LE = le;            // latch;       SR pin 4
            Clk = clk;          // clock;       SR pin 3
            Sdo = sdo;          // data out;    SR pin 22
        }

        /// <summary>
        /// Standard pin bindings for the MBI5027.
        /// </summary>
        public static Mbi5027PinMapping Standard => new Mbi5027PinMapping(25, 12, 16, 20);
        /*
            Data    = 25    // data
            OE      = 12    // blank
            RClk    = 16    // latch / publish storage register
            SrClk   = 20    // storage register clock
            SrClr   = 21    // clear
        */

        /// <summary>
        /// Serial data in pin number.
        /// </summary>
        public int Sdi { get; set; }

        /// <summary>
        /// Serial data out pin number.
        /// Only used for open circuit detection.
        /// </summary>
        public int Sdo { get; set; }

        /// <summary>
        /// OE (output enable) pin number.
        /// </summary>
        public int OE { get; set; }

        /// <summary>
        /// RCLK (latch) pin number.
        /// </summary>
        public int LE { get; set; }

        /// <summary>
        /// SRCLK (shift) pin number.
        /// </summary>
        public int Clk { get; set; }
    }
}
