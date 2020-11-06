// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>
    public enum MotorPort
    {
        /// <summary>Left motor</summary>
        MotorLeft = 0x01,

        /// <summary>Right motor</summary>
        MotorRight = 0x02,

        /// <summary>Both motors</summary>
        Both = MotorLeft + MotorRight
    }
}
