// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// PotentiometerSensor class for analogic potentiometer sensors
    /// </summary>
    public class PotentiometerSensor : AnalogSensor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public PotentiometerSensor(GoPiGo goPiGo, GrovePort port) : base(goPiGo, port)
        { }

        /// <summary>
        /// Get the value as a percent
        /// </summary>
        public override string ToString() => $"{ValueAsPercent} %";

        /// <summary>
        /// Get the sensor name "Potentiometer Sensor"
        /// </summary>
        public new string SensorName => "Potentiometer Sensor";
    }
}
