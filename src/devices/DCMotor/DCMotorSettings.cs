// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
     /// <summary>
     /// Settings for DCMotor class.
     /// </summary>
    public class DCMotorSettings
    {
        /// <summary>
        /// Pin connected to the first data pin in the H-bridge.
        /// </summary>
        public int? Pin0 { get; set; }

        /// <summary>
        /// Pin connected to the second data pin in the H-bridge.
        /// Note for 2-pin setting: if additional settings for PWM need to be used
        /// Set PwmController and other Pwm settings in this class and set Pin1 to null.
        /// </summary>
        public int? Pin1 { get; set; }

        /// <summary>
        /// Is one of the pins connected to the enable input of the H-bridge?
        /// For 3-pin setting it should be set to true.
        /// For 2- or 1-pin setting it should be set to false.
        /// </summary>
        public bool UseEnableAsPwm { get; set; } = true;

        /// <summary>
        /// Controller related to pin operations.
        /// If not set it will construct the default GpioController.
        /// </summary>
        public GpioController Controller { get; set; }

        /// <summary>
        /// PwmController class related to enable pin (3-pin setting).
        /// For 2- or 1-pin mode this allows changing PWM settings
        /// instead of automatically picked SoftPwm.
        /// </summary>
        public PwmController PwmController { get; set; }

        /// <summary>
        /// PWM chip number. For SoftPwm this will be a pin number.
        /// </summary>
        public int PwmChip { get; set; } = 0;

        /// <summary>
        /// PWM channel. For SoftPwm this value is ignored.
        /// </summary>
        public int PwmChannel { get; set; } = 0;

        /// <summary>
        /// PWM frequency. This value will default to 50Hz if not set.
        /// </summary>
        public double PwmFrequency { get; set; } = 50;
    }
}
