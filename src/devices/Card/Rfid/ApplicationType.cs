// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Application type for 106 kbps type B cards
    /// </summary>
    public enum ApplicationType
    {
        Proprietary = 0b0000_0000,
        ApplicationBytesCoded = 0b0000_0100,
    }
}
