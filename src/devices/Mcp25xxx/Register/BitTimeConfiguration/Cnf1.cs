// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.BitTimeConfiguration
{
    /// <summary>
    /// Configuration 1 Register.
    /// </summary>
    public class Cnf1 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Cnf1 class.
        /// </summary>
        /// <param name="baudRatePrescaler">
        /// BRP[5:0]: Baud Rate Prescaler bits.
        /// TQ = 2 x (BRP[5:0] + 1)/FOSC.
        /// </param>
        /// <param name="synchronizationJumpWidthLength">
        /// SJW[1:0]: Synchronization Jump Width Length bits.
        /// </param>
        public Cnf1(byte baudRatePrescaler, JumpWidthLength synchronizationJumpWidthLength)
        {
            if (baudRatePrescaler > 0b0011_1111)
            {
                throw new ArgumentException($"Invalid BRP value {baudRatePrescaler}.", nameof(baudRatePrescaler));
            }

            BaudRatePrescaler = baudRatePrescaler;
            SynchronizationJumpWidthLength = synchronizationJumpWidthLength;
        }

        /// <summary>
        /// Initializes a new instance of the Cnf1 class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public Cnf1(byte value)
        {
            BaudRatePrescaler = (byte)(value & 0b0011_1111);
            SynchronizationJumpWidthLength = (JumpWidthLength)((value & 0b1100_0000) >> 6);
        }

        /// <summary>
        /// Synchronization Jump Width Length.
        /// </summary>
        public enum JumpWidthLength
        {
            /// <summary>
            /// Length = 1 x TQ.
            /// </summary>
            Tqx1 = 0,
            /// <summary>
            /// Length = 2 x TQ.
            /// </summary>
            Tqx2 = 1,
            /// <summary>
            /// Length = 3 x TQ.
            /// </summary>
            Tqx3 = 2,
            /// <summary>
            /// Length = 4 x TQ.
            /// </summary>
            Tqx4 = 3,
        }

        /// <summary>
        /// BRP[5:0]: Baud Rate Prescaler bits.
        /// TQ = 2 x (BRP[5:0] + 1)/FOSC.
        /// </summary>
        public byte BaudRatePrescaler { get; }

        /// <summary>
        /// SJW[1:0]: Synchronization Jump Width Length bits.
        /// </summary>
        public JumpWidthLength SynchronizationJumpWidthLength { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Cnf1;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)((byte)SynchronizationJumpWidthLength << 6);
            value |= BaudRatePrescaler;
            return value;
        }
    }
}
