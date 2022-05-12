// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Specifies errors that occur on the SerialPort object.
    /// </summary>
    public enum SerialError
    {
        /// <summary>
        /// The application tried to transmit a character,
        /// but the output buffer was full.
        /// </summary>
        TXFull = 0x100,

        /// <summary>
        /// An input buffer overflow has occurred.
        /// There is either no room in the input buffer, or a character
        /// was received after the end-of-file (EOF) character.
        /// </summary>
        RXOver = 0x01,

        /// <summary>
        /// A character-buffer overrun has occurred.
        /// The next character is lost.
        /// </summary>
        Overrun = 0x02,

        /// <summary>
        /// The hardware detected a parity error.
        /// </summary>
        RXParity = 0x04,

        /// <summary>
        /// The hardware detected a framing error.
        /// </summary>
        Frame = 0x08,
    }
}
