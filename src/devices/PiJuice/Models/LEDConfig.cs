// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// LED configuration
    /// </summary>
    public class LEDConfig
    {
        /// <summary>
        /// LED designator
        /// </summary>
        public LED LED { get; set; }

        /// <summary>
        /// LED function type
        /// </summary>
        public LEDFunction LedFunction { get; set; }

        /// <summary>
        /// Color for LED
        /// If LedFunction is ChargeStatus
        /// Red - parameter defines color component level of red below 15%
        /// Green - parameter defines color component charge level over 50%
        /// Blue - parameter defines color component for charging(blink) and fully charged states(constant)
        /// Red LED and Green LED will show the charge status between 15% - 50%
        /// </summary>
        public Color RGB { get; set; }
    }
}
