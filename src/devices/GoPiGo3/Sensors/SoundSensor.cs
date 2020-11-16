// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// SoundSensor class to support analogic microphone sensors
    /// </summary>
    public class SoundSensor : AnalogSensor
    {
        /// <summary>
        /// Creates a new instance of <see cref="SoundSensor"/>.
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public SoundSensor(GoPiGo goPiGo, GrovePort port)
            : base(goPiGo, port)
        {
        }

        /// <summary>
        /// Get the sensor name "Sound Sensor"
        /// </summary>
        public new string SensorName => "Sound Sensor";

    }
}
