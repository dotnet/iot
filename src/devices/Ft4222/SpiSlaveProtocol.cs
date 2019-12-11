// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// SPI as slave protocol
    /// </summary>
    internal enum SpiSlaveProtocol
    {
        /// <summary>
        /// With Protocol
        /// </summary>
        WithProtocol = 0,
        /// <summary>
        /// Without Protocol
        /// </summary>
        WithoutProtocol,
        /// <summary>
        /// Never send acknowledge
        /// </summary>
        NeverSendAcknowledge,
    }
}
