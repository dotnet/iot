// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Describes LED colors in an LED matrix or bargraph.
    /// </summary>
    public enum LedColor
    {
        /// <summary>
        /// Disable LED.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Enable red LED.
        /// </summary>
        Red = 1,

        /// <summary>
        /// Enable green LED.
        /// </summary>
        Green = 2,

        /// <summary>
        /// Enable both green and red LEDs, producing a yellow color.
        /// </summary>
        Yellow = 3
    }
}
