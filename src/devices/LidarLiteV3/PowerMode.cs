// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DistanceSensor.Models.LidarLiteV3
{
    /// <summary>
    /// Power mode flags
    /// </summary>
    [Flags]
    public enum PowerMode
    {
        /// <summary>
        /// Turn on the device.
        /// </summary>
        On = 0x80,

        /// <summary>
        /// Disable the receiver circuit.
        /// </summary>
        DisableReceiverCircuit = 0x00,

        /// <summary>
        /// Put the device to sleep.
        /// </summary>
        Sleep = 0x04
    }
}
