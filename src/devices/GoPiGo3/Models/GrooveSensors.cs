// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.GoPiGo3.Models
{
    public class GrooveSensor
    {
        /// <summary>
        /// Constructor for the groove sensor part of the main GoPiGo3 class
        /// </summary>
        /// <param name="groovePort">The Groove port, either Groove1 or Groove2</param>
        public GrooveSensor(GroovePort groovePort)
        {
            if (!((groovePort != GroovePort.Groove1) || (groovePort != GroovePort.Groove2)))
                throw new ArgumentException("Groove sensor can only be on Port 1 or Port 2");
            SensorType = GrooveSensorType.None;
        }

        public GroovePort Port { get; }
        public GrooveSensorType SensorType { get; set; }
        public byte I2cDataLength { get; set; }
    }

    /// <summary>
    /// The type of Groove sensors
    /// </summary>
    public enum GrooveSensorType
    {
        None = 0,
        Custom = 1,
        InfraredRemote,
        InfraredEV3Remote,
        Ultrasonic,
        I2c
    }

    /// <summary>
    /// The state of the Goove element
    /// </summary>
    public enum GrooveSensorState
    {
        ValidData,
        NotConfigured,
        Configuring,
        NoData,
        I2cError
    }    
}
