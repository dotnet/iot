// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Flags that indicate motor status
    /// </summary>
    public enum MotorStatusFlags
    {
        /// <summary>No problems with motors detected</summary>
        AllOk = 0,

        /// <summary>The motors are automatically disabled because the battery voltage is too low.</summary>
        LowVoltageFloat = 0x01,

        /// <summary>The motors aren't close to the target (applies to position control and dps speed control).</summary>
        Overloaded = 0x02
    }

    /// <summary>
    /// Sets the speed for the motor.
    /// </summary>
    public enum MotorSpeed : byte
    {
        /// <summary>Motor is stopped</summary>
        Stop = 0,

        /// <summary>Motor is at full speed</summary>
        Full = 100,

        /// <summary>Motor is at half speed</summary>
        Half = 50,
        // Actually any value great than 100 will float motors

        /// <summary>Motor is floating</summary>
        Float = 128
    }
}
