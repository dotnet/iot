using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// SoundSensor class to support basic Grove microphones
    /// </summary>
    class SoundSensor : AnalogSensor
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
