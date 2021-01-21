// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// The serial UART speed in baud
    /// </summary>
    public enum SerialSpeed
    {
        /// <summary>7200 baud</summary>
        B7200 = 0xFA,

        /// <summary>9600 baud</summary>
        B9600 = 0xEB,

        /// <summary>14400 baud</summary>
        B14400 = 0xDA,

        /// <summary>19200 baud</summary>
        B19200 = 0xCB,

        /// <summary>38400 baud</summary>
        B38400 = 0xAB,

        /// <summary>57600 baud</summary>
        B57600 = 0x9A,

        /// <summary>115200 baud</summary>
        B115200 = 0x7A,

        /// <summary>128000 baud</summary>
        B128000 = 0x74,

        /// <summary>230400 baud</summary>
        B230400 = 0x5A,

        /// <summary>460800 baud</summary>
        B460800 = 0x3A,

        /// <summary>921600 baud</summary>
        B921600 = 0x1C,

        /// <summary>1228800 baud</summary>
        B1228800 = 0x15,
    }
}
