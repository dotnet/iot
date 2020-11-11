// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// Interface for a sensor
    /// </summary>
    public interface ISensor<T>
    {
        /// <summary>
        /// Property to get the raw value of the sensor as an int
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Returns the raw value of the sensort as a formated string
        /// </summary>
        string ToString();

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        string SensorName { get; }

        /// <summary>
        /// grove sensor port
        /// </summary>
        GrovePort Port { get; }
    }
}
