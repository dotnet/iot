// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Sensor ports 1, 2, 3 and 4
    /// </summary>
    public enum SensorPort : byte
    {
        // Used to select the ports for sensors

        /// <summary>Port A</summary>
        PortA = 0,

        /// <summary>Port B</summary>
        PortB = 1,

        /// <summary>Port C</summary>
        PortC = 2,

        /// <summary>Port D</summary>
        PortD = 3,
    }
}
