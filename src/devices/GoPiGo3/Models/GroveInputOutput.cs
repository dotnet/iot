// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The setup of the Grove element
    /// </summary>
    public enum GroveInputOutput
    {
        /// <summary>Digital input</summary>
        InputDigital = 0,

        /// <summary>Digital output</summary>
        OutputDigital,

        /// <summary>Digital input with pull up</summary>
        InputDigitalPullUp,

        /// <summary>Digital input with pull down</summary>
        InputDigitalPullDown,

        /// <summary>Analog input</summary>
        InputAnalog,

        /// <summary>PWM output</summary>
        OutputPwm,

        /// <summary>Analog input with pull up</summary>
        InputAnalogPullUp,

        /// <summary>Analog input with pull down</summary>
        InputAnalogPullDown
    }
}
