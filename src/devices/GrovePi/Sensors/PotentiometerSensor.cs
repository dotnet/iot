// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// Potentiometer sensor
    /// </summary>
    public class PotentiometerSensor : AnalogSensor
    {
        /// <summary>
        /// Potentiometer sensor constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public PotentiometerSensor(GrovePi grovePi, GrovePort port)
            : base(grovePi, port)
        {
        }

        /// <summary>
        /// Returns the value as a percent from 0 % to 100 %
        /// </summary>
        public override string ToString() => $"{ValueAsPercent} %";

        /// <summary>
        /// Get the name Potentiometer Sensor
        /// </summary>
        public new string SensorName => "Potentiometer Sensor";
    }
}
