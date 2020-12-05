// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Power input state
    /// </summary>
    public enum PowerInState
    {
        /// <summary>
        /// No power supply connected
        /// </summary>
        NotPresent = 0,

        /// <summary>
        /// Find an alternative power supply with a higher rating
        /// </summary>
        Bad,

        /// <summary>
        /// Power supply cannot charge the PiJuice and provide power to the Raspberry Pi
        /// </summary>
        Weak,

        /// <summary>
        /// Power is good
        /// </summary>
        Present,
    }
}
