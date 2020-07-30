// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents pin bindings for the Sn74hc595.
    /// </summary>
    public struct Sn74hc595PinMapping
    {
        /// <param name="data">Data pin</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="rclk">Register clock pin (latch)</param>
        /// <param name="srclk">Shift register pin (shift to data register)</param>
        /// <param name="srclr">Shift register clear pin (shift register is cleared)</param>
        public Sn74hc595PinMapping(int data, int oe, int rclk, int srclk, int srclr)
        {
            Data = data;            // data in;     SR pin 14
            OE = oe;                // blank;       SR pin 13
            RClk = rclk;            // latch;       SR pin 12
            SrClk = srclk;          // clock;       SR pin 11
            SrClr = srclr;          // clear;       SR pin 10
                                    // daisy chain  SR pin 9 (QH` not mapped; for SR -> SR communication)
        }

        /// <summary>
        /// Standard pin bindings for the Sn74hc595.
        /// </summary>
        public static Sn74hc595PinMapping Standard => new Sn74hc595PinMapping(25, 12, 16, 20, 21);
        /*
            Data    = 25    // data
            OE      = 12    // blank
            RClk    = 16    // latch / publish storage register
            SrClk   = 20    // storage register clock
            SrClr   = 21    // clear
        */

        /// <summary>
        /// Matching pin bindings for the Sn74hc595 (Pi and shift register pin numbers match).
        /// </summary>
        public static Sn74hc595PinMapping Matching => new Sn74hc595PinMapping(14, 13, 12, 11, 10);
        /*
            Data    = 14    // data
            OE      = 13    // blank
            RClk    = 12    // latch / publish storage register
            SrClk   = 11    // storage register clock
            SrClr   = 10    // clear
        */

        /// <summary>
        /// SER (data) pin number.
        /// </summary>
        public int Data { get; set; }

        /// <summary>
        /// OE (output enable) pin number.
        /// </summary>
        public int OE { get; set; }

        /// <summary>
        /// RCLK (latch) pin number.
        /// </summary>
        public int RClk { get; set; }

        /// <summary>
        /// SRCLK (shift) pin number.
        /// </summary>
        public int SrClk { get; set; }

        /// <summary>
        /// SRCLR (clear register) pin number.
        /// </summary>
        public int SrClr { get; set; }
    }
}
