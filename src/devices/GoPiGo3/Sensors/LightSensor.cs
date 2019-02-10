// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// LightSensor class for analogic luminosity sensors
    /// </summary>
    public class LightSensor:AnalogSensor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Groove Port, need to be in the list of SupportedPorts</param>
        public LightSensor(GoPiGo goPiGo, GroovePort port):base(goPiGo, port)
        { }

        /// <summary>
        /// Get the sensor name "Light Sensor"
        /// </summary>
        public new string SensorName => "Light Sensor";
    }
}
