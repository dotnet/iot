// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.GrovePiDevice.Models
{
    /// <summary>
    /// GrovePi info
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Instantiate Info object.
        /// </summary>
        /// <param name="softwareVersion">Firmware version.</param>
        public Info(Version softwareVersion)
        {
            SoftwareVersion = softwareVersion;
        }

        /// <summary>
        /// Manufacturer information
        /// </summary>
        public string Manufacturer => "Dexter Industries";

        /// <summary>
        /// Board information
        /// </summary>
        public string Board => "GrovePi+";

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version SoftwareVersion { get; set; }
    }
}
