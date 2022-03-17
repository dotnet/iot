// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Specifies the type of character that was received on the serial port of the SerialPort object.
    /// </summary>
    public enum SerialData
    {
        /// <summary>
        /// A character was received and placed in the input buffer.
        /// In the Windows implementation, this is EV_RXCHAR.
        /// </summary>
        Chars = 0x01,

        /// <summary>
        /// The end of file character was received and placed in the input buffer.
        /// In the Windows implementation, this is EV_RXFLAG.
        /// </summary>
        Eof = 0x02
    }
}
