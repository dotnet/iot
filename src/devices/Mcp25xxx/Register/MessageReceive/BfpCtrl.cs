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
        /// <param name="rx0bfPinOperationMode">
        /// B0BFM: Rx0BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB0.
        /// False = Digital Output mode.
        /// </param>
        /// <param name="rx1bfPinOperationMode">
        /// B1BFM: Rx1BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB1.
        /// False = Digital Output mode.
        /// </param>
        /// <param name="rx0bfPinFunctionEnable">
        /// B0BFE: Rx0BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B0BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </param>
        /// <param name="rx1bfPinFunctionEnable">
        /// B1BFE: Rx1BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B1BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </param>
        /// <param name="rx0bfPinState">
        /// B0BFS: Rx0BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx0BF is configured as an interrupt pin.
        /// </param>
        /// <param name="rx1bfPinState">
        /// B1BFS: Rx1BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx1BF is configured as an interrupt pin.
        /// </param>
        public BfpCtrl(
            bool rx0bfPinOperationMode,
            bool rx1bfPinOperationMode,
            bool rx0bfPinFunctionEnable,
            bool rx1bfPinFunctionEnable,
            bool rx0bfPinState,
            bool rx1bfPinState)
        {
            Rx0bfPinOperationMode = rx0bfPinOperationMode;
            Rx1bfPinOperationMode = rx1bfPinOperationMode;
            Rx0bfPinFunctionEnable = rx0bfPinFunctionEnable;
            Rx1bfPinFunctionEnable = rx1bfPinFunctionEnable;
            Rx0bfPinState = rx0bfPinState;
            Rx1bfPinState = rx1bfPinState;
        }

        /// <summary>
        /// Initializes a new instance of the BfpCtrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public BfpCtrl(byte value)
        {
            Rx0bfPinOperationMode = (value & 1) == 1;
            Rx1bfPinOperationMode = ((value >> 1) & 1) == 1;
            Rx0bfPinFunctionEnable = ((value >> 2) & 1) == 1;
            Rx1bfPinFunctionEnable = ((value >> 3) & 1) == 1;
            Rx0bfPinState = ((value >> 4) & 1) == 1;
            Rx1bfPinState = ((value >> 5) & 1) == 1;
        }

        /// <summary>
        /// B0BFM: Rx0BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB0.
        /// False = Digital Output mode.
        /// </summary>
        public bool Rx0bfPinOperationMode { get; }

        /// <summary>
        /// B1BFM: Rx1BF Pin Operation mode bit.
        /// True = Pin is used as an interrupt when a valid message is loaded into RXB1.
        /// False = Digital Output mode.
        /// </summary>
        public bool Rx1bfPinOperationMode { get; }

        /// <summary>
        /// B0BFE: Rx0BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B0BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool Rx0bfPinFunctionEnable { get; }

        /// <summary>
        /// B1BFE: Rx1BF Pin Function Enable bit.
        /// True = Pin function is enabled, operation mode is determined by the B1BFM bit.
        /// False = Pin function is disabled, pin goes to the high-impedance state.
        /// </summary>
        public bool Rx1bfPinFunctionEnable { get; }

        /// <summary>
        /// B0BFS: Rx0BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx0BF is configured as an interrupt pin.
        /// </summary>
        public bool Rx0bfPinState { get; }

        /// <summary>
        /// B1BFS: Rx1BF Pin State bit (Digital Output mode only).
        /// Reads as '0' when Rx1BF is configured as an interrupt pin.
        /// </summary>
        public bool Rx1bfPinState { get; }

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

            if (Rx0bfPinOperationMode)
            {
                value |= 0b0000_0001;
            }

            if (Rx1bfPinOperationMode)
            {
                value |= 0b0000_0010;
            }

            if (Rx0bfPinFunctionEnable)
            {
                value |= 0b0000_0100;
            }

            if (Rx1bfPinFunctionEnable)
            {
                value |= 0b0000_1000;
            }

            if (Rx0bfPinState)
            {
                value |= 0b0001_0000;
            }

            if (Rx1bfPinState)
            {
                value |= 0b0010_0000;
            }

            return value;
        }
    }
}
