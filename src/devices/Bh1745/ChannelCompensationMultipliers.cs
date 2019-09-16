// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Channel compensation multipliers used to compensate the 4 color channels of the Bh1745.
    /// </summary>
    public class ChannelCompensationMultipliers
    {
        /// <summary>
        /// Multiplier for the red color channel.
        /// </summary>
        public double Red { get; set; }

        /// <summary>
        /// Multiplier for the green color channel.
        /// </summary>
        public double Green { get; set; }

        /// <summary>
        /// Multiplier for the blue color channel.
        /// </summary>
        public double Blue { get; set; }

        /// <summary>
        /// Multiplier for the clear color channel.
        /// </summary>
        public double Clear { get; set; }

    }
}
