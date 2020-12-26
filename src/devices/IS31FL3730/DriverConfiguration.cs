// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// Represents the configuration register status for an IS31FL3730 LED Matrix controller.
    /// </summary>
    public struct DriverConfiguration
    {
        /// <summary>
        /// Indicates whether the controller is active or in soft-shutdown mode.
        /// </summary>
        public bool IsShutdown;

        /// <summary>
        /// Indicates whether the matrix intensity is controlled via configuration registers or audio input.
        /// </summary>
        public bool IsAudioInputEnabled;

        /// <summary>
        /// Matrix Layout (8x8, 7x5, 6x10 or 5x11)
        /// </summary>
        public MatrixLayout Layout;

        /// <summary>
        /// Identifies which of the two available matrices are active.
        /// </summary>
        public MatrixMode Mode;

        /// <summary>
        /// Sets the LED Drive Strength in mA.
        /// </summary>
        public DriveStrength DriveStrength;
    }
}
