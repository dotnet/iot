// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="interruptFlagCode">ICOD[2:0]: Interrupt Flag Code bits.</param>
        /// <param name="operationMode">OPMOD[2:0]: Operation Mode bits.</param>
        public CanStat(FlagCode interruptFlagCode, OperationMode operationMode)
        {
            InterruptFlagCode = interruptFlagCode;
            OperationMode = operationMode;
        }

        /// <summary>
        /// Initializes a new instance of the CanStat class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanStat(byte value)
        {
            InterruptFlagCode = (FlagCode)((value & 0b0000_1110) >> 1);
            OperationMode = (OperationMode)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Interrupt Flag Code.
        /// </summary>
        public enum FlagCode
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
        /// ICOD[2:0]: Interrupt Flag Code bits.
        /// </summary>
        public FlagCode InterruptFlagCode { get; }

        /// <summary>
        /// OPMOD[2:0]: Operation Mode bits.
        /// </summary>
        public OperationMode OperationMode { get; }

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
            byte value = (byte)((byte)OperationMode << 5);
            value |= (byte)((byte)InterruptFlagCode << 1);
            return value;
        }
    }
}
