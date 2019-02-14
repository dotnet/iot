// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.GoPiGo3.Models
{
    public class GroveSensor
    {
        /// <summary>
        /// Constructor for the Grove sensor part of the main GoPiGo3 class
        /// </summary>
        /// <param name="GrovePort">The Grove port, either Grove1 or Grove2</param>
        public GroveSensor(GrovePort GrovePort)
        {
            if ((GrovePort != GrovePort.Grove1) && (GrovePort != GrovePort.Grove2))
                throw new ArgumentException("Grove sensor can only be on Port 1 or Port 2");
            SensorType = GroveSensorType.None;
        }

        public GrovePort Port { get; }
        public GroveSensorType SensorType { get; set; }
        public byte I2cDataLength { get; set; }
    }

    /// <summary>
    /// The type of Grove sensors
    /// </summary>
    public enum GroveSensorType
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
    public enum GroveSensorState
    {
        ValidData,
        NotConfigured,
        Configuring,
        NoData,
        I2cError
    }
}
