// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Serial port baud rates
    /// </summary>
    public enum BaudRate
    {
        /// <summary>
        /// 9600
        /// </summary>
        B0009600 = 0x00,
        /// <summary>
        /// 19200
        /// </summary>
        B0019200 = 0x01,
        /// <summary>
        /// 38400
        /// </summary>
        B0038400 = 0x02,
        /// <summary>
        /// 57600
        /// </summary>
        B0057600 = 0x03,
        /// <summary>
        /// 115200
        /// </summary>
        B0115200 = 0x04,
        /// <summary>
        /// 230400
        /// </summary>
        B0230400 = 0x05,
        /// <summary>
        /// 460800
        /// </summary>
        B0460800 = 0x06,
        /// <summary>
        /// 921600
        /// </summary>
        B0921600 = 0x07,
        /// <summary>
        /// 1288000
        /// </summary>
        B1288000 = 0x08
    }
}
