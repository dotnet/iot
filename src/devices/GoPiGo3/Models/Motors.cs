// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.GoPiGo3.Models
{

    /// <summary>
    /// flags -- 8-bits of bit-flags that indicate motor status:
    /// bit 0 -- LowVoltageFloat - The motors are automatically disabled because the battery voltage is too low
    /// bit 1 -- Overloaded - The motors aren't close to the target (applies to position control and dps speed control).
    /// </summary> 
    public enum MotorStatusFlags
    {
        AllOk = 0,
        LowVoltageFloat = 0x01,
        Overloaded = 0x02
    }

    /// <summary>
    /// Set quickly a speed for the motor
    /// </summary>
    public enum MotorSpeed : byte
    {
        Stop = 0,
        Full = 100,
        Half = 50,
        // Motros in float mode
        // Actually any value great than 100 will float motors
        Float = 128
    }
}
