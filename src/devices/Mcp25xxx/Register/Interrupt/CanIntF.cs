// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.Interrupt
{
    /// <summary>
    /// CAN Interrupt Flag Register.
    /// </summary>
    public class CanIntF
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
        /// Receive Buffer 0 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Rx0If { get; set; }

        /// <summary>
        /// Receive Buffer 1 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Rx1If { get; set; }

        /// <summary>
        /// Transmit Buffer 0 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx0If { get; set; }

        /// <summary>
        /// Transmit Buffer 1 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx1If { get; set; }

        /// <summary>
        /// Transmit Buffer 2 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Tx2If { get; set; }

        /// <summary>
        /// Error Interrupt Flag bit (multiple sources in the EFLG register).
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool ErrIf { get; set; }

        /// <summary>
        /// Wake-up Interrupt Flag bit.
        /// True = Interrupt pending(must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool WakIf { get; set; }

        /// <summary>
        /// Message Error Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool Merrf { get; set; }
    }
}
