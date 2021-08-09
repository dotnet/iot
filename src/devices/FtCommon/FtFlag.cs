// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Flags for the device status
    /// </summary>
    [Flags]
    public enum FtFlag : uint
    {
        /// <summary>
        /// Indicates that the device is open
        /// </summary>
        PortOpened = 0x00000001,

        /// <summary>
        /// Indicates that the device is enumerated as a hi-speed USB device
        /// </summary>
        HiSpeedMode = 0x00000002,
    }
}
