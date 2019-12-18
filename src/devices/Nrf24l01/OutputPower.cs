// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// nRF24l01 Output Power
    /// </summary>
    public enum OutputPower
    {
        /// <summary>
        /// -18dBm
        /// </summary>
        N18dBm = 0x00,

        /// <summary>
        /// -12dBm
        /// </summary>
        N12dBm = 0x01,

        /// <summary>
        /// -6dBm
        /// </summary>
        N06dBm = 0x02,

        /// <summary>
        /// 0dBm
        /// </summary>
        N00dBm = 0x03
    }
}
