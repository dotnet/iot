// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// RxnBF Pin Control and Status Register.
    /// </summary>
    public class BfpCtrl : IRegister
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
        /// Initializes a new instance of the BfpCtrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public BfpCtrl(byte value)
        {
            B0Bfm = (value & 1) == 1;
            B1Bfm = ((value >> 1) & 1) == 1;
            B0Bfe = ((value >> 2) & 1) == 1;
            B1Bfe = ((value >> 3) & 1) == 1;
            B0Bfs = ((value >> 4) & 1) == 1;
            B1Bfs = ((value >> 5) & 1) == 1;
        }

        /// <summary>
        /// Rx0BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB0.
        /// False = Digital Output mode.
        /// </summary>
        public bool B0Bfm { get; }

        /// <summary>
        /// Rx1BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB1.
        /// False = Digital Output mode.
        /// </summary>
        public bool B1Bfm { get; }

        /// <summary>
        /// Rx0BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B0BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool B0Bfe { get; }

        /// <summary>
        /// Rx1BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B1BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool B1Bfe { get; }

        /// <summary>
        /// Rx0BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx0BF is configured as an interrupt pin.
        /// </summary>
        public bool B0Bfs { get; }

        /// <summary>
        /// Rx1BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx1BF is configured as an interrupt pin.
        /// </summary>
        public bool B1Bfs { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.BfpCtrl;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (B0Bfm)
            {
                value |= 0b0000_0001;
            }

            if (B1Bfm)
            {
                value |= 0b0000_0010;
            }

            if (B0Bfe)
            {
                value |= 0b0000_0100;
            }

            if (B1Bfe)
            {
                value |= 0b0000_1000;
            }

            if (B0Bfs)
            {
                value |= 0b0001_0000;
            }

            if (B1Bfs)
            {
                value |= 0b0010_0000;
            }

            return value;
        }
    }
}
