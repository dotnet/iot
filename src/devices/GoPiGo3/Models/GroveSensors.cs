// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// Represents Grove sensor
    /// </summary>
    public class GroveSensor
    {
        /// <summary>
        /// Constructor for the Grove sensor part of the main GoPiGo3 class
        /// </summary>
        /// <param name="GrovePort">The Grove port, either Grove1 or Grove2</param>
        public GroveSensor(GrovePort GrovePort)
        {
            if ((GrovePort != GrovePort.Grove1) && (GrovePort != GrovePort.Grove2))
            {
                throw new ArgumentException("Grove sensor can only be on Port 1 or Port 2");
            }

            SensorType = GroveSensorType.None;
            Port = GrovePort;
        }

        /// <summary>
        /// Grove Port
        /// </summary>
        public GrovePort Port { get; }

        /// <summary>
        /// Grove sensor type
        /// </summary>
        /// <value></value>
        public GroveSensorType SensorType { get; set; }

        /// <summary>I2C data length</summary>
        public byte I2cDataLength { get; set; }
    }

    /// <summary>
    /// The type of Grove sensors
    /// </summary>
    public enum GroveSensorType
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>Custom</summary>
        Custom = 1,

        /// <summary>Infrared remote</summary>
        InfraredRemote,

        /// <summary>Infrared EV3 remote</summary>
        InfraredEV3Remote,

        /// <summary>Ultrasonic</summary>
        Ultrasonic,

        /// <summary>I2C</summary>
        I2c
    }

    /// <summary>
    /// The state of the Goove element
    /// </summary>
    public enum GroveSensorState
    {
        /// <summary>Valid data</summary>
        ValidData,

        /// <summary>Not configured</summary>
        NotConfigured,

        /// <summary>Configuring</summary>
        Configuring,

        /// <summary>No data</summary>
        NoData,

        /// <summary>I2C error</summary>
        I2cError
    }
}
