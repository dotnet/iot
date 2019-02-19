// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;
using System.Device.Gpio;

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
        public override string ToString() => Value == PinValue.High ? "On" : "Off";

        /// <summary>
        /// Get the name Led
        /// </summary>
        public new string SensorName => "Led";
    }
}
