// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Different event types that can be triggered by the GPIO.
    /// </summary>
    [Flags]
    public enum PinEventTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Triggered when pin value goes from 0 to 1
        /// </summary>
        Rising = 1,
        /// <summary>
        /// Triggered when a pin value goes from 1 to 0
        /// </summary>
        Falling = 2
    }
}
