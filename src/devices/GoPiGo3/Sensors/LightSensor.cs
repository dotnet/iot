// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// LightSensor class for analogic luminosity sensors
    /// </summary>
    public class LightSensor : AnalogSensor
    {
        /// <summary>
        /// Creates a new instance of <see cref="LightSensor"/>.
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public LightSensor(GoPiGo goPiGo, GrovePort port)
            : base(goPiGo, port)
        {
        }

        /// <summary>
        /// Get the sensor name "Light Sensor"
        /// </summary>
        public new string SensorName => "Light Sensor";
    }
}
