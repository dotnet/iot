// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents pin bindings for the Sn74hc595.
    /// Requires specifying 3 pins (serial data in, data clock, and latch).
    /// Can specify output enable pin (otherwise, wire to ground).
    /// </summary>
    public struct ShiftRegisterPinMapping
    {
        // Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5168.pdf

        /// <param name="sdi">Serial data in pin</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="le">Register clock pin (latch)</param>
        /// <param name="clk">Shift register pin (shift to data register)</param>
        public ShiftRegisterPinMapping(int sdi, int clk, int le, int oe = 0)
        {
            Sdi = sdi;              // serial data in
            Clk = clk;              // storage register clock
            LE = le;                // shift register latch
            OE = oe;                // output enable / disable (blank)
        }

        /// <summary>
        /// Standard pin bindings for the Sn74hc595.
        /// </summary>
        public static ShiftRegisterPinMapping Standard => new ShiftRegisterPinMapping(16, 20, 21, 12);
        /*
            Sdi   = 16    // data
            Clk   = 20    // storage register clock
            LE    = 21    // latch / publish storage register
            OE    = 12    // blank
        */

        /// <summary>
        /// Serial data in pin.
        /// </summary>
        public int Sdi { get; set; }

        /// <summary>
        /// Storage register clock pin.
        /// </summary>
        public int Clk { get; set; }

        /// <summary>
        /// Shift register clock pin.
        /// </summary>
        public int LE { get; set; }

        /// <summary>
        /// Output enable pin.
        /// </summary>
        public int OE { get; set; }
    }
}
