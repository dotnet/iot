// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice info
    /// </summary>
    /// <param name="FirmwareVersion">Firmware version</param>
    public record PiJuiceInfo(Version FirmwareVersion)
    {
        /// <summary>
        /// Manufacturer information
        /// </summary>
        public string Manufacturer => "PiJuice";

        /// <summary>
        /// Board information
        /// </summary>
        public string Board => "PiJuice";
    }
}
