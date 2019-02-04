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
        public int? Pin0 { get; set; }
        public int? Pin1 { get; set; }
        public bool UseEnableAsPwm { get; set; } = true;
        
        public GpioController Controller { get; set; }

        public PwmController PwmController { get; set; }
        public int PwmChip { get; set; } = 0;
        public int PwmChannel { get; set; } = 0;
        public double PwmFrequency { get; set; } = 50;
    }
}