// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents pin bindings for the Sn74hc595.
    /// Requires specifying 3 pins (data, data clock, and latch).
    /// Can specify output enable pin (otherwise, must wire to ground).
    /// </summary>
    public struct ShiftRegisterPinMapping
    {
        /// <param name="data">Data pin</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="rclk">Register clock pin (latch)</param>
        /// <param name="srclk">Shift register pin (shift to data register)</param>
        public ShiftRegisterPinMapping(int data, int oe, int rclk, int srclk)
        {
            Data = data;            // data in;     SR pin 14
            OE = oe;                // blank;       SR pin 13
            RClk = rclk;            // latch;       SR pin 12
            SrClk = srclk;          // clock;       SR pin 11
        }

        /// <summary>
        /// Standard pin bindings for the Sn74hc595.
        /// </summary>
        public static ShiftRegisterPinMapping Standard => new ShiftRegisterPinMapping(25, 12, 16, 20);
        /*
            Data    = 25    // data
            OE      = 12    // blank
            RClk    = 16    // latch / publish storage register
            SrClk   = 20    // storage register clock
            SrClr   = 21    // clear
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
    }
}
