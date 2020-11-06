// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Information for the various sensor ID, firmware and bootloader versions
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Instantiates an Info object
        /// <param name="chipId">Chip identifier</param>
        /// <param name="acceleratorId">Accelerometer identifier</param>
        /// <param name="magnetometerId">Magnetometer identifier</param>
        /// <param name="gyroscopeId">Gyroscope identifier</param>
        /// <param name="firmwareVersion">Firmware version</param>
        /// <param name="bootloaderVersion">Bootloader version</param>
        /// </summary>
        public Info(byte chipId, byte acceleratorId, byte magnetometerId, byte gyroscopeId, Version firmwareVersion, Version bootloaderVersion)
        {
            ChipId = ChipId;
            AcceleratorId = acceleratorId;
            MagnetometerId = magnetometerId;
            GyroscopeId = gyroscopeId;
            FirmwareVersion = firmwareVersion;
            BootloaderVersion = bootloaderVersion;
        }

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
