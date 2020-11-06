// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Application type for 106 kbps type B cards
    /// </summary>
    public enum ApplicationType
    {
        /// <summary>
        /// Proprietary application type
        /// </summary>
        Proprietary = 0b0000_0000,

        /// <summary>
        /// Byte coded application type
        /// </summary>
        ApplicationBytesCoded = 0b0000_0100,
    }
}
