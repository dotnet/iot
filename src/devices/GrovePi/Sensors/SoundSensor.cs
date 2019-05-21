// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// SoundSensor class to support basic Grove microphones
    /// </summary>
    public class SoundSensor : AnalogSensor
    {
        /// <summary>
        /// SoundSensor constructor
        /// </summary>
        /// <param name="grovePi">The GoPiGo3 class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public SoundSensor(GrovePi grovePi, GrovePort port) : base(grovePi, port)
        { }

        /// <summary>
        /// Get the name Sound Sensor
        /// </summary>
        public new string SensorName => "Sound Sensor";
    }
}
