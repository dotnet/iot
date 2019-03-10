// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Tests.Register.CanControl;

namespace Iot.Device.Mcp25xxx.Register.CanControl
{
    /// <summary>
    /// CAN Status Register.
    /// </summary>
    public class CanStat : IRegister
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
        /// Initializes a new instance of the CanStat class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanStat(byte value)
        {
            Icod = (InterruptFlagCode)((value & 0b0000_1110) >> 1);
            OpMod = (OperationMode)((value & 0b1110_0000) >> 5);
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
            TxB0 = 3,
            /// <summary>
            /// TXB1 interrupt.
            /// </summary>
            TxB1 = 4,
            /// <summary>
            /// TXB2 interrupt.
            /// </summary>
            TxB2 = 5,
            /// <summary>
            /// RXB0 interrupt.
            /// </summary>
            RxB0 = 6,
            /// <summary>
            /// RXB1 interrupt.
            /// </summary>
            RxB1 = 7
        }

        /// <summary>
        /// Interrupt Flag Code bits.
        /// </summary>
        public InterruptFlagCode Icod { get; }

        /// <summary>
        /// Operation Mode bits.
        /// </summary>
        public OperationMode OpMod { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.CanStat;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)((byte)OpMod << 5);
            value |= (byte)((byte)Icod << 1);
            return value;
        }
    }
}
