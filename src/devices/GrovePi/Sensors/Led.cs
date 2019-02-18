using Iot.Device.GrovePiDevice.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// Led class for simple Led
    /// </summary>
    public class Led : DigitalOutput
    {
        /// <summary>
        /// Led constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public Led(GrovePi grovePi, GrovePort port) : base(grovePi, port)
        { }

        /// <summary>
        /// Returns On if the led is on, Off otherwise
        /// </summary>
        /// <returns>Returns On if the led is on, Off otherwise</returns>
        public override string ToString() => Value == PinLevel.High ? "On" : "Off";

        /// <summary>
        /// Get the name Led
        /// </summary>
        public new string SensorName => "Led";
    }
}
