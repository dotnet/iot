// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice info
    /// </summary>
    public class PiJuiceInfo
    {
        /// <summary>
        /// Manufacturer information
        /// </summary>
        public string Manufacturer => "PiJuice";

        /// <summary>
        /// Board information
        /// </summary>
        public string Board => "PiJuice";

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version SoftwareVersion { get; set; }
    }
}
