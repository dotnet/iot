using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Defines on which events the frequency counter should increase.
    /// </summary>
    public enum FrequencyMode
    {
        /// <summary>
        /// Don't change the value
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// Trigger when the value is low
        /// </summary>
        Low = 1,

        /// <summary>
        /// Trigger when the value is high. This mode is only supported on some boards.
        /// </summary>
        High = 2,

        /// <summary>
        /// Trigger on a rising edge
        /// </summary>
        Rising = 3,

        /// <summary>
        /// Trigger on a falling edge
        /// </summary>
        Falling = 4,

        /// <summary>
        /// Trigger on both edges. Note that this will typically result in a frequency reported twice as high as it actually is.
        /// </summary>
        Change = 5,
    }
}
