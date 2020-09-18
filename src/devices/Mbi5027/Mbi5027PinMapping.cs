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
        /// <param name="clk">Shift register pin (shift to data register)</param>
        /// <param name="le">Register clock pin (latch)</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="sdo">Serial data out pin -- required for open circuit detection</param>
        public Mbi5027PinMapping(int sdi, int clk, int le, int oe = -1, int sdo = -1)
        {
            Sdi = sdi;
            Clk = clk;
            LE = le;
            OE = oe;
            Sdo = sdo;
        }

        /// <summary>
        /// Minimal pin bindings for the MBI5027.
        /// </summary>
        public static Mbi5027PinMapping Minimal => new Mbi5027PinMapping(16, 20, 21);
        /*
            Sdi   = 16      SR 2   -- serial data in
            Clk   = 20      SR 3   -- storage register clock
            LE    = 21      SR 4   -- enable latch to publish storage register
        */

        /// <summary>
        /// Complete pin bindings for the MBI5027.
        /// </summary>
        public static Mbi5027PinMapping Complete => new Mbi5027PinMapping(16, 20, 21, 12, 25);
        /*
            Sdi   = 16      SR 2   -- serial data in
            Clk   = 20      SR 3   -- storage register clock
            LE    = 21      SR 4   -- enable latch to publish storage register
            OE    = 12      SR 21  -- output enable or disable
            Sdo   = 25      SR 22  -- Serial data out - required for open circuit detection
        */

        /// <summary>
        /// Serial data in pin.
        /// </summary>
        public int Sdi { get; set; }

        /// <summary>
        /// Serial data out pin.
        /// Only used (directly) for open circuit detection.
        /// </summary>
        public int Sdo { get; set; }

        /// <summary>
        /// OE (output enable) pin .
        /// </summary>
        public int OE { get; set; }

        /// <summary>
        /// LE (shift register clock/latch) pin.
        /// </summary>
        public int LE { get; set; }

        /// <summary>
        /// Clk (data register clock) pin number.
        /// </summary>
        public int Clk { get; set; }
    }
}
