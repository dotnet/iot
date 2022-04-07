// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Type of a message returned from the board
    /// </summary>
    public enum ReplyType
    {
        /// <summary>
        /// Not a valid message
        /// </summary>
        None = 0,

        /// <summary>
        /// A sysex message was received. This is the default.
        /// The message buffer contains the binary reply data without the sysex byte.
        /// </summary>
        SysexCommand = 1,

        /// <summary>
        /// A text message was received.
        /// This may happen if a special script runs on the firmata device that prints raw ASCII characters to the console.
        /// One occasion where this also happens is if an ESP32 is printing a crash dump.
        /// The payload contains the raw message in unicode bytes.
        /// </summary>
        AsciiData = 2,
    }
}
