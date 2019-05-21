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
        public byte ChipId { get; set; }
        public byte AcceleratorId { get; set; }
        public byte MagnetometerId { get; set; }
        public byte GyroscopeId { get; set; }
        public Version FirmwareVersion { get; set; }
        public Version BootloaderVersion { get; set; }
    }
}
