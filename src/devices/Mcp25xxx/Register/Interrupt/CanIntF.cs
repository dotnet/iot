// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.Interrupt
{
    /// <summary>
    /// CAN Interrupt Flag Register.
    /// </summary>
    public class CanIntF : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the CanIntF class.
        /// </summary>
        /// <param name="rx0if">
        /// Receive Buffer 0 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="rx1if">
        /// Receive Buffer 1 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="tx0if">
        /// Transmit Buffer 0 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="tx1if">
        /// Transmit Buffer 1 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="tx2if">
        /// Transmit Buffer 2 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="errif">
        /// Error Interrupt Flag bit (multiple sources in the EFLG register).
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="wakif">
        /// Wake-up Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="merrf">
        /// Message Error Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        public CanIntF(bool rx0if, bool rx1if, bool tx0if, bool tx1if, bool tx2if, bool errif, bool wakif, bool merrf)
        {
            Rx0If = rx0if;
            Rx1If = rx1if;
            Tx0If = tx0if;
            Tx1If = tx1if;
            Tx2If = tx2if;
            ErrIf = errif;
            WakIf = wakif;
            Merrf = merrf;
        }

        /// <summary>
        /// Initializes a new instance of the CanIntF class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanIntF(byte value)
        {
            Rx0If = (value & 1) == 1;
            Rx1If = ((value >> 1) & 1) == 1;
            Tx0If = ((value >> 2) & 1) == 1;
            Tx1If = ((value >> 3) & 1) == 1;
            Tx2If = ((value >> 4) & 1) == 1;
            ErrIf = ((value >> 5) & 1) == 1;
            WakIf = ((value >> 6) & 1) == 1;
            Merrf = ((value >> 7) & 1) == 1;
        }

        /// <summary>
        /// Receive Buffer 0 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Rx0If { get; }

        /// <summary>
        /// Receive Buffer 1 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Rx1If { get; }

        /// <summary>
        /// Transmit Buffer 0 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx0If { get; }

        /// <summary>
        /// Transmit Buffer 1 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx1If { get; }

        /// <summary>
        /// Transmit Buffer 2 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx2If { get; }

        /// <summary>
        /// Error Interrupt Flag bit (multiple sources in the EFLG register).
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool ErrIf { get; }

        /// <summary>
        /// Wake-up Interrupt Flag bit.
        /// True = Interrupt pending(must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool WakIf { get; }

        /// <summary>
        /// Message Error Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Merrf { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.CanIntF;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (Rx0If)
            {
                value |= 0b0000_0001;
            }

            if (Rx1If)
            {
                value |= 0b0000_0010;
            }

            if (Tx0If)
            {
                value |= 0b0000_0100;
            }

            if (Tx1If)
            {
                value |= 0b0000_1000;
            }

            if (Tx2If)
            {
                value |= 0b0001_0000;
            }

            if (ErrIf)
            {
                value |= 0b0010_0000;
            }

            if (WakIf)
            {
                value |= 0b0100_0000;
            }

            if (Merrf)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
