// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// SPI as replica protocol
    /// </summary>
    internal enum SpiReplicaProtocol
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
