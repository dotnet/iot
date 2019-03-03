// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.CanControl
{
    /// <summary>
    /// CAN Status Register.
    /// </summary>
    public class CanStat
    {
        /// <summary>
        /// Initializes a new instance of the CanStat class.
        /// </summary>
        /// <param name="icod">Interrupt Flag Code bits.</param>
        /// <param name="opMod">Operation Mode bits.</param>
        public CanStat(InterruptFlagCode icod, OperationMode opMod)
        {
            Icod = icod;
            OpMod = opMod;
        }

        /// <summary>
        /// Interrupt Flag Code.
        /// </summary>
        public enum InterruptFlagCode
        {
            /// <summary>
            /// No Interrupt.
            /// </summary>
            No = 0,
            /// <summary>
            /// Error interrupt.
            /// </summary>
            Error = 1,
            /// <summary>
            /// Wake-up interrupt.
            /// </summary>
            WakeUp = 2,
            /// <summary>
            /// TXB0 interrupt.
            /// </summary>
            TXB0 = 3,
            /// <summary>
            /// TXB1 interrupt.
            /// </summary>
            TXB1 = 4,
            /// <summary>
            /// TXB2 interrupt.
            /// </summary>
            TXB2 = 5,
            /// <summary>
            /// RXB0 interrupt.
            /// </summary>
            RXB0 = 6,
            /// <summary>
            /// RXB1 interrupt.
            /// </summary>
            RXB1 = 7
        }

        /// <summary>
        /// Operation Mode.
        /// </summary>
        public enum OperationMode
        {
            /// <summary>
            /// Device is in the Normal Operation mode.
            /// </summary>
            NormalOperation = 0,
            /// <summary>
            /// Device is in Sleep mode.
            /// </summary>
            Sleep = 1,
            /// <summary>
            /// Device is in Loopback mode.
            /// </summary>
            Loopback = 2,
            /// <summary>
            /// Device is in Listen-Only mode.
            /// </summary>
            ListenOnly = 3,
            /// <summary>
            /// Device is in Configuration mode.
            /// </summary>
            Configuration = 4
        }

        /// <summary>
        /// Interrupt Flag Code bits.
        /// </summary>
        public InterruptFlagCode Icod { get; set; }

        /// <summary>
        /// Operation Mode bits.
        /// </summary>
        public OperationMode OpMod { get; set; }
    }
}
