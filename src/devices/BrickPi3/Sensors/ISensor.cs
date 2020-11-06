// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Interface for a sensor
    /// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Property to return the raw value of the sensor as an int
        /// </summary>
        int Value { get; }

        /// <summary>
        /// Property to return the raw value of the sensort as a string
        /// </summary>
        string ValueAsString { get; }

        /// <summary>
        /// To update sensors
        /// </summary>
        void UpdateSensor(object? state);

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>
        /// The value as a string
        /// </returns>
        string ReadAsString();

        /// <summary>
        /// Reads the sensor values as a raw int value
        /// </summary>
        /// <returns>The value as a int</returns>
        int ReadRaw();

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        string GetSensorName();

        /// <summary>
        /// Selects the next mode.
        /// </summary>
        void SelectNextMode();

        /// <summary>
        /// Selects the previous mode.
        /// </summary>
        void SelectPreviousMode();

        /// <summary>
        /// Numbers the of modes.
        /// </summary>
        /// <returns>The number of modes</returns>
        int NumberOfModes();

        /// <summary>
        /// Returned the name of the selectd mode
        /// </summary>
        /// <returns>The mode.</returns>
        string SelectedMode();

        /// <summary>
        /// Sensor port
        /// </summary>
        /// <returns>The sensor port</returns>
        SensorPort Port { get; }

    }
}
