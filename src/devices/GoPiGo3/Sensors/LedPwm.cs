// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// LedPwm class to control led thru PWM
    /// </summary>
    public class LedPwm : Buzzer
    {
        /// <summary>
        /// Constructor for the LedPwm class
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public LedPwm(GoPiGo goPiGo, GrovePort port)
            : this(goPiGo, port, 0)
        {
        }

        /// <summary>
        /// Constructor for the LedPwm class
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="duty">The duty cycle for the led, 100 = full bright, 0 = full dark</param>
        public LedPwm(GoPiGo goPiGo, GrovePort port, byte duty)
            : base(goPiGo, port, duty)
        {
        }

        /// <summary>
        /// Get the sensor name "Led PWM"
        /// </summary>
        public new string SensorName => "Led PWM";
    }
}
