// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.CanControl
{
    /// <summary>
    /// CAN Control Register.
    /// </summary>
    public class CanCtrl
    {
        /// <summary>
        /// Initializes a new instance of the CanCtrl class.
        /// </summary>
        /// <param name="clkpre">CLKOUT Pin Prescaler bits.</param>
        /// <param name="clken">
        /// CLKOUT Pin Enable bit.
        /// True = CLKOUT pin is enabled.
        /// False = CLKOUT pin is disabled(pin is in a high-impedance state).
        /// </param>
        /// <param name="osm">
        /// One-Shot Mode bit.
        /// True = Enabled: Message will only attempt to transmit one time.
        /// False = Disabled: Messages will reattempt transmission if required.
        /// </param>
        /// <param name="abat">
        /// Abort All Pending Transmissions bit.
        /// True = Requests abort of all pending transmit buffers.
        /// False = Terminates request to abort all transmissions.
        /// </param>
        /// <param name="reqop">Request Operation mode bits.</param>
        public CanCtrl(ClkOutPinPrescaler clkpre, bool clken, bool osm, bool abat, RequestOperationMode reqop)
        {
            ClkPre = clkpre;
            ClkEn = clken;
            Osm = osm;
            Abat = abat;
            ReqOp = reqop;
        }

        /// <summary>
        /// CLKOUT Pin Prescaler.
        /// </summary>
        public enum ClkOutPinPrescaler
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
            ClockDivideBy4 = 3,
            /// <summary>
            /// FCLKOUT = System Clock/8.
            /// </summary>
            ClockDivideBy8 = 4 
        }

        /// <summary>
        /// Request Operation mode.
        /// </summary>
        public enum RequestOperationMode
        {
            /// <summary>
            /// Sets Normal Operation mode.
            /// </summary>
            NormalOperation = 0,
            /// <summary>
            /// Sets Sleep mode.
            /// </summary>
            Sleep = 1,
            /// <summary>
            /// Sets Loopback mode.
            /// </summary>
            Loopback = 2,
            /// <summary>
            /// Sets Listen-Only mode.
            /// </summary>
            ListenOnly = 3,
            /// <summary>
            /// Sets Configuration mode.
            /// </summary>
            Configuration = 4
        }

        /// <summary>
        /// CLKOUT Pin Prescaler bits.
        /// </summary>
        public ClkOutPinPrescaler ClkPre { get; set; }

        /// <summary>
        /// CLKOUT Pin Enable bit.
        /// True = CLKOUT pin is enabled.
        /// False = CLKOUT pin is disabled (pin is in a high-impedance state).
        /// </summary>
        public bool ClkEn { get; set; }

        /// <summary>
        /// One-Shot Mode bit.
        /// True = Enabled: Message will only attempt to transmit one time.
        /// False = Disabled: Messages will reattempt transmission if required.
        /// </summary>
        public bool Osm { get; set; }

        /// <summary>
        /// Abort All Pending Transmissions bit.
        /// True = Requests abort of all pending transmit buffers.
        /// False = Terminates request to abort all transmissions.
        /// </summary>
        public bool Abat { get; set; }

        /// <summary>
        /// Request Operation mode bits.
        /// </summary>
        public RequestOperationMode ReqOp { get; set; }
    }
}
