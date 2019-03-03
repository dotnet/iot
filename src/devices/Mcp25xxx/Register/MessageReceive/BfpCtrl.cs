// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// RxnBF Pin Control and Status Register.
    /// </summary>
    public class BfpCtrl
    {
        /// <summary>
        /// Initializes a new instance of the BfpCtrl class.
        /// </summary>
        /// <param name="b0bfm">
        /// Rx0BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB0.
        /// False = Digital Output mode.
        /// </param>
        /// <param name="b1bfm">
        /// Rx1BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB1.
        /// False = Digital Output mode.
        /// </param>
        /// <param name="b0bfe">
        /// Rx0BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B0BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </param>
        /// <param name="b1bfe">
        /// Rx1BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B1BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </param>
        /// <param name="b0bfs">
        /// Rx0BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx0BF is configured as an interrupt pin.
        /// </param>
        /// <param name="b1bfs">
        /// Rx1BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx1BF is configured as an interrupt pin.
        /// </param>
        public BfpCtrl(bool b0bfm, bool b1bfm, bool b0bfe, bool b1bfe, bool b0bfs, bool b1bfs)
        {
            B0Bfm = b0bfm;
            B1Bfm = b1bfm;
            B0Bfe = b0bfe;
            B1Bfe = b1bfe;
            B0Bfs = b0bfs;
            B1Bfs = b1bfs;
        }

        /// <summary>
        /// Rx0BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB0.
        /// False = Digital Output mode.
        /// </summary>
        public bool B0Bfm { get; set; }

        /// <summary>
        /// Rx1BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB1.
        /// False = Digital Output mode.
        /// </summary>
        public bool B1Bfm { get; set; }

        /// <summary>
        /// Rx0BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B0BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool B0Bfe { get; set; }

        /// <summary>
        /// Rx1BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B1BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool B1Bfe { get; set; }

        /// <summary>
        /// Rx0BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx0BF is configured as an interrupt pin.
        /// </summary>
        public bool B0Bfs { get; set; }

        /// <summary>
        /// Rx1BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx1BF is configured as an interrupt pin.
        /// </summary>
        public bool B1Bfs { get; set; }
    }
}
