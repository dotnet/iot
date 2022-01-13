// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
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
        /// Gets true if the sensor is connected.
        /// </summary>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Creates a sensor
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="type">The sensor type.</param>
        protected internal Sensor(Brick brick, SensorPort port, SensorType type)
        {
            Brick = brick;
            Port = port;
            SensorType = type;
        }

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        public virtual string SensorName { get => "Generic sensor"; }

        /// <summary>
        /// Gets the Sensor port
        /// </summary>
        /// <returns>The sensor port</returns>
        public SensorPort Port { get; internal set; }

        /// <summary>
        /// Gets the sensor type
        /// </summary>
        public SensorType SensorType { get; internal set; }

        /// <summary>
        /// To notify a property has changed. It means the value has changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// To notify a property has been updated. It means the property has been updated regardeless of a change in its value.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyUpdated;

        /// <summary>
        /// Raises the on property changed event.
        /// </summary>
        /// <param name="name">The property name.</param>
        protected internal void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Raises the on property updated event.
        /// </summary>
        /// <param name="name">The property name.</param>
        protected internal void OnPropertyUpdated(string name) => PropertyUpdated?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
