// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// The MCP28XXX family has an address mapping concept for accessing registers.
    /// This provides a way to easily address registers by group or type. This is only
    /// relevant for 16-bit devices where it has two banks (Port A and B) of 8-bit
    /// GPIO pins.
    /// </summary>
    public enum BankStyle
    {
        /// <summary>
        /// This mode is used specifically for 16-bit devices where it treats the
        /// two 8-bit banks as one 16-bit bank.
        /// </summary>
        /// <remarks>
        /// Each of the registers are interleaved so that sending two bytes in a
        /// row will set the equivalent register for the second bank. This way you
        /// can set all 16 GPIO pins/settings with one command sequence.
        ///
        /// Note that this behavior is also dependent on the default behavior
        /// of IOCON.SEQOP = 0 (the default) which automatically increments the
        /// register address as bytes come in.
        ///
        /// This is IOCON.BANK = 0 and is the default.
        /// </remarks>
        Sequential = 0,

        /// <summary>
        /// This mode keeps the two 8-bit banks registers separate.
        /// </summary>
        /// <remarks>
        /// While this keeps the register addresses for bank A the same as the
        /// 8-bit controllers it requires sending a separate command sequence to
        /// set all 16-bits as the second bank's register addresses are not
        /// sequential.
        ///
        /// Changing IOCON.SEQOP to 1 (not the default) will cause the
        /// register address pointer to toggle between Port A and B for the
        /// given register if in this mode.
        ///
        /// This is IOCON.BANK = 1.
        /// </remarks>
        Separated = 1
    }
}
