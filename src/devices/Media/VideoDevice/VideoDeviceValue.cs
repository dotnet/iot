// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The default and current values of a video device's control.
    /// </summary>
    public class VideoDeviceValue
    {
        /// <summary>
        /// Instantiate VideoDeviceValue.
        /// </summary>
        /// <param name="name">Control's name.</param>
        /// <param name="minimum">Minimum value.</param>
        /// <param name="maximum">Maximum value.</param>
        /// <param name="step">Value change step.</param>
        /// <param name="defaultValue">Control's default value.</param>
        /// <param name="currentValue">Control's current value.</param>
        public VideoDeviceValue(string name, int minimum, int maximum, int step, int defaultValue, int currentValue)
        {
            Name = name;
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
            DefaultValue = defaultValue;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Control's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Minimum value
        /// </summary>
        public int Minimum { get; set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        public int Maximum { get; set; }

        /// <summary>
        /// Value change step size
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        /// Control's default value
        /// </summary>
        public int DefaultValue { get; set; }

        /// <summary>
        /// Control's current value
        /// </summary>
        public int CurrentValue { get; set; }
    }
}
