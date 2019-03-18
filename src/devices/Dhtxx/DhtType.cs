// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// The type of DHT sensor used
    /// </summary>
    public enum DhtType
    {
        Dht11,
        Dht12,
        /// <summary>
        /// AM2301
        /// </summary>
        Dht21,
        /// <summary>
        /// AM2302
        /// </summary>
        Dht22
    }
}
