// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// A simple light like the train ones.
    /// </summary>
    public class PassiveLight : Sensor
    {
        private bool _isOn;
        private int _brightness;

        /// <summary>
        /// Sets the brightness from 0 to 100.
        /// </summary>
        public int Brightness { get => _brightness; set => SetBrightness(value); }

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        public override string SensorName { get => "Passive light"; }

        /// <summary>
        /// PAssive light.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        protected internal PassiveLight(Brick brick, SensorPort port)
                : base(brick, port, SensorType.SimpleLights)
        {
        }

        /// <summary>
        /// Sets the brigthness from 0 to 100.
        /// </summary>
        /// <param name="brightness">The brightness from 0 to 100.</param>
        public void SetBrightness(int brightness)
        {
            if ((brightness < 0) || (brightness > 100))
            {
                throw new ArgumentException("Brightness can only be between 0 (off) and 100 (full bright)");
            }

            _brightness = brightness;
            if (_isOn)
            {
                On();
            }
        }

        /// <summary>
        /// Switch on the light.
        /// </summary>
        public void On()
        {
            Brick.SetMotorPower(Port, Brightness);
            _isOn = true;
        }

        /// <summary>
        /// Switches on the light with a specific brightness.
        /// </summary>
        /// <param name="brightness">The brightness from 0 to 100.</param>
        public void On(int brightness)
        {
            SetBrightness(brightness);
            On();
        }

        /// <summary>
        /// Switechs off the light.
        /// </summary>
        public void Off()
        {
            SetBrightness(0);
            _isOn = false;
        }
    }
}
