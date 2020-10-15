// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Button event types
    /// </summary>
    public enum ButtonEventType
    {
        /// <summary>
        /// No button event assigned
        /// </summary>
        NoEvent = 0,

        /// <summary>
        /// Triggered immediately after button is pressed
        /// </summary>
        Press,

        /// <summary>
        /// Triggered immediately after button is released
        /// </summary>
        Release,

        /// <summary>
        /// Triggered if button is released in time less than configurable timeout after button press
        /// </summary>
        SinglePress,

        /// <summary>
        /// Triggered if button is double pressed in time less than configurable timeout
        /// </summary>
        DoublePress,

        /// <summary>
        /// Triggered if button is hold pressed hold for configurable time period 1
        /// </summary>
        LongPress1,

        /// <summary>
        /// Triggered if button is hold pressed hold for configurable time period 2
        /// </summary>
        LongPress2,

        /// <summary>
        /// Unknown button event
        /// </summary>
        Unknown = 1000
    }
}
