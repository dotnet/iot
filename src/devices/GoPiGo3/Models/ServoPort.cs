// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Select the servo motor port
    /// </summary>
    public enum ServoPort
    {
        /// <summary>Servo 1</summary>
        Servo1 = 0x01,

        /// <summary>Servo 2</summary>
        Servo2 = 0x02,

        /// <summary>Both servos</summary>
        Both = Servo1 + Servo2
    }
}
