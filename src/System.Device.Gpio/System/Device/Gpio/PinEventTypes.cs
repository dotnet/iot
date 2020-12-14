// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// Event types that can be triggered by the GPIO.
    /// Also used to report the received event types back.
    /// </summary>
    [Flags]
    public enum PinEventTypes
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Triggered when pin value goes from low to high.
        /// </summary>
        Rising = 1,

        /// <summary>
        /// Triggered when a pin value goes from high to low.
        /// </summary>
        Falling = 2
    }
}
