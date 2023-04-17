// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Am2320
{
    /// <summary>
    /// Device information.
    /// </summary>
    public class DeviceInformation
    {
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public ushort Model { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        public uint DeviceId { get; set; }
    }
}
