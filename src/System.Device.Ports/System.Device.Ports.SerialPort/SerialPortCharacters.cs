// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Common characters used in the serial port
    /// </summary>
    public enum SerialPortCharacters : byte
    {
        /// <summary>
        /// The default value for XON
        /// </summary>
        DefaultXONChar = (byte)17,

        /// <summary>
        /// The default value for XOFF
        /// </summary>
        DefaultXOFFChar = (byte)19,

        /// <summary>
        /// The default value for EOF
        /// </summary>
        EOFChar = (byte)26,

        /// <summary>
        /// The default value for parity replace
        /// </summary>
        QuestionMark = (byte)'?',
    }
}
