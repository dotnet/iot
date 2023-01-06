// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3DhAccelerometer
{
    /// <summary>
    /// Operating mode selection
    /// </summary>
    public enum OperatingMode : byte
    {
        /// <summary>
        /// Power down mode
        /// </summary>
        HighResolutionMode,

        /// <summary>
        /// Normal mode
        /// </summary>
        NormalMode,

        /// <summary>
        /// Low power mode
        /// </summary>
        LowPowerMode,
    }
}
