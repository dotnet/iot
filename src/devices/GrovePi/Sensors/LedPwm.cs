using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// LedPwm class to add PWM on a Led
    /// </summary>
    public class LedPwm : PwmOutput
    {
        /// <summary>
        /// LedPwm constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public LedPwm(GrovePi grovePi, GrovePort port) : base(grovePi, port)
        { }

        /// <summary>
        /// Returns the duty cycle as percentage from 0 % to 100 %
        /// </summary>
        /// <returns>Returns the duty cycle as percentage from 0 % to 100 %</returns>
        public override string ToString() => $"{ValueAsPercent} %";

        /// <summary>
        /// Get the name Led PWM
        /// </summary>
        public new string SensorName => "Led PWM";
    }
}
