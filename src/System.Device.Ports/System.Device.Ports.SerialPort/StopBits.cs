// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Specifies the number of stop bits used on the SerialPort object.
    /// </summary>
    public enum StopBits
    {
        /// <summary>
        /// No stop bits are used.
        /// </summary>
        None = 0,

        /// <summary>
        /// One stop bit is used.
        /// </summary>
        One = 1,

        /// <summary>
        /// Two stop bits are used.
        /// </summary>
        Two = 2,

        /// <summary>
        /// 1.5 stop bits are used.
        /// </summary>
        OnePointFive = 3
    }
}
