// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// LedNormal class to control a normal led on a digital output
    /// </summary>
    public class Led : Relay
    {
        /// <summary>
        /// Constructor of the normal Led class
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public Led(GoPiGo goPiGo, GrovePort port)
            : this(goPiGo, port, false)
        {
        }

        /// <summary>
        /// Constructor of the normal Led class
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="inverted">If the led is inverted, off when level if high and off when level is low</param>
        public Led(GoPiGo goPiGo, GrovePort port, bool inverted)
            : base(goPiGo, port, inverted)
        {
        }

        /// <summary>
        /// Get the sensor name "Led"
        /// </summary>
        public new string SensorName => "Led";
    }
}
