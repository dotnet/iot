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
        /// <param name="brp">Synchronization Jump Width Length bits.</param>
        /// <param name="sjw">
        /// Baud Rate Prescaler bits.
        /// TQ = 2 x (BRP[5:0] + 1)/FOSC.
        /// </param>
        public Cnf1(byte brp, SynchronizationJumpWidthLength sjw)
        {
            if (brp > 0b0011_1111)
            {
                throw new ArgumentException($"Invalid BRP value {brp}.", nameof(brp));
            }

            Brp = brp;
            Sjw = sjw;
        }

        /// <summary>
        /// Synchronization Jump Width Length.
        /// </summary>
        public enum SynchronizationJumpWidthLength
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
        /// Baud Rate Prescaler bits.
        /// TQ = 2 x (BRP[5:0] + 1)/FOSC.
        /// </summary>
        public byte Brp { get; }

        /// <summary>
        /// Synchronization Jump Width Length bits.
        /// </summary>
        public SynchronizationJumpWidthLength Sjw { get; }

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
            byte value = (byte)((byte)Sjw << 6);
            value |= Brp;
            return value;
        }
    }
}
