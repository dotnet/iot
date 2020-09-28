// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Usb.Enumerations
{
    /// <summary>
    /// Cable connection status of the USB-C port.
    /// </summary>
    public enum UsbCCableConnection
    {
        /// <summary>No cable is connected.</summary>
        Disconnected = 0,

        /// <summary>The cable is attached to CC1.</summary>
        CC1 = 1,

        /// <summary>The cable is attached to CC2.</summary>
        CC2 = 2
    }
}
