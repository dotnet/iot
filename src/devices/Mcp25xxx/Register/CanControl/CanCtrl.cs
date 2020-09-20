// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Tests.Register.CanControl;

namespace Iot.Device.Mcp25xxx.Register.CanControl
{
    /// <summary>
    /// CAN Control Register.
    /// </summary>
    public class CanCtrl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the CanCtrl class.
        /// </summary>
        /// <param name="clkOutPinPrescaler">CLKPRE[1:0]: CLKOUT Pin Prescaler bits.</param>
        /// <param name="clkOutPinEnable">
        /// CLKEN: CLKOUT Pin Enable bit.
        /// True = CLKOUT pin is enabled.
        /// False = CLKOUT pin is disabled(pin is in a high-impedance state).
        /// </param>
        /// <param name="oneShotMode">
        /// OSM: One-Shot Mode bit.
        /// True = Enabled: Message will only attempt to transmit one time.
        /// False = Disabled: Messages will reattempt transmission if required.
        /// </param>
        /// <param name="abortAllPendingTransmissions">
        /// ABAT: Abort All Pending Transmissions bit.
        /// True = Requests abort of all pending transmit buffers.
        /// False = Terminates request to abort all transmissions.
        /// </param>
        /// <param name="requestOperationMode">REQOP[2:0]: Request Operation mode bits.</param>
        public CanCtrl(
            PinPrescaler clkOutPinPrescaler,
            bool clkOutPinEnable,
            bool oneShotMode,
            bool abortAllPendingTransmissions,
            OperationMode requestOperationMode)
        {
            ClkOutPinPrescaler = clkOutPinPrescaler;
            ClkOutPinEnable = clkOutPinEnable;
            OneShotMode = oneShotMode;
            AbortAllPendingTransmissions = abortAllPendingTransmissions;
            RequestOperationMode = requestOperationMode;
        }

        /// <summary>
        /// Initializes a new instance of the CanCtrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanCtrl(byte value)
        {
            ClkOutPinPrescaler = (PinPrescaler)(value & 0b0000_0011);
            ClkOutPinEnable = ((value >> 2) & 1) == 1;
            OneShotMode = ((value >> 3) & 1) == 1;
            AbortAllPendingTransmissions = ((value >> 4) & 1) == 1;
            RequestOperationMode = (OperationMode)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// CLKOUT Pin Prescaler.
        /// </summary>
        public enum PinPrescaler
        {
            /// <summary>
            /// FCLKOUT = System Clock/1.
            /// </summary>
            ClockDivideBy1 = 0,

            /// <summary>
            /// FCLKOUT = System Clock/2.
            /// </summary>
            ClockDivideBy2 = 1,

            /// <summary>
            /// FCLKOUT = System Clock/4.
            /// </summary>
            ClockDivideBy4 = 2,

            /// <summary>
            /// FCLKOUT = System Clock/8.
            /// </summary>
            ClockDivideBy8 = 3
        }

        /// <summary>
        /// CLKPRE[1:0]: CLKOUT Pin Prescaler bits.
        /// </summary>
        public PinPrescaler ClkOutPinPrescaler { get; }

        /// <summary>
        /// CLKEN: CLKOUT Pin Enable bit.
        /// True = CLKOUT pin is enabled.
        /// False = CLKOUT pin is disabled (pin is in a high-impedance state).
        /// </summary>
        public bool ClkOutPinEnable { get; }

        /// <summary>
        /// OSM: One-Shot Mode bit.
        /// True = Enabled: Message will only attempt to transmit one time.
        /// False = Disabled: Messages will reattempt transmission if required.
        /// </summary>
        public bool OneShotMode { get; }

        /// <summary>
        /// ABAT: Abort All Pending Transmissions bit.
        /// True = Requests abort of all pending transmit buffers.
        /// False = Terminates request to abort all transmissions.
        /// </summary>
        public bool AbortAllPendingTransmissions { get; }

        /// <summary>
        /// REQOP[2:0]: Request Operation mode bits.
        /// </summary>
        public OperationMode RequestOperationMode { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.CanCtrl;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)((byte)RequestOperationMode << 5);

            if (AbortAllPendingTransmissions)
            {
                value |= 0b0001_0000;
            }

            if (OneShotMode)
            {
                value |= 0b0000_1000;
            }

            if (ClkOutPinEnable)
            {
                value |= 0b0000_0100;
            }

            value |= (byte)ClkOutPinPrescaler;
            return value;
        }
    }
}
