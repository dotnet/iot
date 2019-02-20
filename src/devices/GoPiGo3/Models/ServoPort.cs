// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Select the servo motor port
    /// </summary>
    public enum ServoPort
    {
        Servo1 = 0x01,
        Servo2 = 0x02,
        Both = Servo1 + Servo2
    }
}
