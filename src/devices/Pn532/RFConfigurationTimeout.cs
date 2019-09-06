// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The Radio Frequency configuration timeout
    /// </summary>
    public enum RFConfigurationTimeout
    {
        /// <summary>
        /// NoTimeout
        /// </summary>
        NoTimeout = 0x00,
        /// <summary>
        /// Timeout 100 Micro Second
        /// </summary>
        Timeout100MicroSecond = 0x01,
        /// <summary>
        /// Timeout 200 Micro Second
        /// </summary>
        Timeout200MicroSecond = 0x02,
        /// <summary>
        /// Timeout 400 Micro Second
        /// </summary>
        Timeout400MicroSecond = 0x03,
        /// <summary>
        /// Timeout 800 Micro Second
        /// </summary>
        Timeout800MicroSecond = 0x04,
        /// <summary>
        /// Timeout 1.6 ms
        /// </summary>
        Timeout1Dot6ms = 0x05,
        /// <summary>
        /// Timeout 3.2 ms
        /// </summary>
        Timeout3Dot2ms = 0x06,
        /// <summary>
        /// Timeout 6.4 ms
        /// </summary>
        Timeout6Dot4ms = 0x07,
        /// <summary>
        /// Timeout 12.8 ms
        /// </summary>
        Timeout12Dot8ms = 0x08,
        /// <summary>
        /// Timeout 25.6 ms
        /// </summary>
        Timeout25Dot6ms = 0x09,
        /// <summary>
        /// Timeout 51.2 ms
        /// </summary>
        Timeout51Dot2ms = 0x0A,
        /// <summary>
        /// Timeout 102.4 ms
        /// </summary>
        Timeout102Dot4ms = 0x0B,
        /// <summary>
        /// Timeout 204.8 ms
        /// </summary>
        Timeout204Dot8ms = 0x0C,
        /// <summary>
        /// Timeout 409.6 ms
        /// </summary>
        Timeout409Dot6ms = 0x0D,
        /// <summary>
        /// Timeout 819.2 ms
        /// </summary>
        Timeout819Dot2ms = 0x0E,
        /// <summary>
        /// Timeout 1.64 sec
        /// </summary>
        Timeout1Dot64sec = 0x0F,
        /// <summary>
        /// Timeout 3.28 sec
        /// </summary>
        Timeout3Dot28sec = 0x10,
    }
}
