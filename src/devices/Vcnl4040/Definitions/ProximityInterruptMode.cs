// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of available proximity interrupt modes.
    /// These modes combine the choices for regular interrupt events
    /// and the logic output mode.
    /// </summary>
    public enum ProximityInterruptMode
    {
        /// <summary>
        /// PS neither triggers regular interrupts nor runs in logic output mode.
        /// </summary>
        Nothing,

        /// <summary>
        /// Close proximity: PS triggers an interrupt event when the sensor detects
        /// an approach where the upper threshold, approached from below, is exceeded.
        /// </summary>
        CloseInterrupt,

        /// <summary>
        /// Long-distance: PS triggers an interrupt event when the sensor detects
        /// a removal where the lower threshold, approached from above, is undershot.
        /// </summary>
        AwayInterrupt,

        /// <summary>
        /// Close proximity and long-distance:
        /// PS triggers an interrupt event when the sensor detects either
        /// an approach where the upper threshold, approached from below, is exceeded
        /// or a removal where the lower threshold, approached from above, is undershot.
        /// </summary>
        CloseOrAwayInterrupt,

        /// <summary>
        /// The sensor operates in Logic Output mode, where, contrary to
        /// regular interrupt operation, the INT pin signals whether
        /// the sensor has detected an approach between the set thresholds.
        /// </summary>
        LogicOutput,
    }
}
