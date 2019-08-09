using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Media
{
    /// <summary>
    /// The default and current values of a video device's control.
    /// </summary>
    public class VideoDeviceValue
    {
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
