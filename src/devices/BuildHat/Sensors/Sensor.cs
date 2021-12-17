// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Interface for a sensor
    /// </summary>
    public class Sensor
    {
        internal Brick Brick;

        /// <summary>
        /// Property to return the raw value of the sensor as an int
        /// </summary>
        public int Value { get; internal set; }

        /// <summary>
        /// Property to return the raw value of the sensort as a string
        /// </summary>
        public string ValueAsString { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets true if the motor is connected.
        /// </summary>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Creates a sensor
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="type">The sensor type.</param>
        public Sensor(Brick brick, SensorPort port, SensorType type)
        {
            Brick = brick;
            Port = port;
            SensorType = type;
        }

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>
        /// The value as a string
        /// </returns>
        public string ReadAsString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Reads the sensor values as a raw int value
        /// </summary>
        /// <returns>The value as a int</returns>
        public int ReadRaw()
        {
            return -1;
        }

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        public string GetSensorName() => "Generic sensor";

        /// <summary>
        /// Gets the Sensor port
        /// </summary>
        /// <returns>The sensor port</returns>
        public SensorPort Port { get; internal set; }

        /// <summary>
        /// Gets the sensor type
        /// </summary>
        public SensorType SensorType { get; internal set; }
    }
}
