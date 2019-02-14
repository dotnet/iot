// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System.Collections.Generic;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
	/// Interface for a sensor 
	/// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Property to get the raw value of the sensor as an int
        /// </summary>
        int Value { get; }

        /// <summary>
        /// Property to return the raw value of the sensort as a string
        /// </summary>
        string ToString();

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        string SensorName { get; }

        /// <summary>
        /// Sensor port
        /// </summary>
        /// <returns>The sensor port</returns>
        GrovePort Port { get; }
    }
}
