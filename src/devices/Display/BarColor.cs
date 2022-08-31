// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Describes LED colors in a Bargraph.
    /// </summary>
    public enum BarColor
    {
        /// <summary>
        /// Disable LED.
        /// </summary>
        OFF = 0,

        /// <summary>
        /// Enable red LED.
        /// </summary>
        RED = 1,

        /// <summary>
        /// Enable green LED.
        /// </summary>
        GREEN = 2,

        /// <summary>
        /// Enable both green and red LEDs, producing a yellow color.
        /// </summary>
        YELLOW = 3
    }
}
