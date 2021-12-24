// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// A simple passive button.
    /// </summary>
    public class ButtonSensor : Sensor
    {
        private bool _isPressed;

        /// <summary>
        /// Gets true when the button is pressed.
        /// </summary>
        public bool IsPressed
        {
            get => _isPressed;
            internal set
            {
                if (_isPressed != value)
                {
                    _isPressed = value;
                    OnPropertyChanged(nameof(IsPressed));
                }

                OnPropertyUpdated(nameof(IsPressed));
            }
        }

        /// <inheritdoc/>
        public override string SensorName => "Button sensor";

        /// <summary>
        /// Button sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        protected internal ButtonSensor(Brick brick, SensorPort port)
                    : base(brick, port, SensorType.ButtonOrTouchSensor)
        {
        }
    }
}
