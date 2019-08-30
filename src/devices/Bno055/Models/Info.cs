// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Information for the various sensor ID, firmware and bootloader versions
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Chip identifier
        /// </summary>
        public byte ChipId { get; set; }

        /// <summary>
        /// Accelerometer identifier
        /// </summary>
        public byte AcceleratorId { get; set; }

        /// <summary>
        /// Magnetometer identifier
        /// </summary>
        public byte MagnetometerId { get; set; }

        /// <summary>
        /// Gyroscope identifier
        /// </summary>
        public byte GyroscopeId { get; set; }

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version FirmwareVersion { get; set; }

        /// <summary>
        /// Bootloader version
        /// </summary>
        public Version BootloaderVersion { get; set; }
    }
}
