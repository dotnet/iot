// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Led function type
    /// </summary>
    public enum LedFunction
    {
        /// <summary>
        /// Led is not configured
        /// </summary>
        NotUsed = 0,

        /// <summary>
        /// Led is configured to signal current charge level of battery
        /// For level less than or equal too 15% red with configurable brightness
        /// For level greater than 15% and level less than or equal to 50% mix of red and green with configurable brightness
        /// For level greater than 50% green with configurable brightness.
        /// When battery is charging blinking blue with configurable brightness is added to current charge level color. For full buttery state blue component is steady on
        /// </summary>
        ChargeStatus,

        /// <summary>
        /// Led is configured as user Led
        /// </summary>
        UserDefined
    }
}
