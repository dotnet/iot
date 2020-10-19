// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Determine how the battery temperature is taken
    /// </summary>
    public enum BatteryTempSense
    {
        /// <summary>
        /// No temperature sensor will be used
        /// </summary>
        NotUsed = 0,

        /// <summary>
        /// Use batteries built-in NTC as per battery NTC terminal
        /// </summary>
        NegativeTemperatureCoefficientB,

        /// <summary>
        /// Use temperature sensor on MCU
        /// </summary>
        OnBoard,

        /// <summary>
        /// Let the PiJuice software determine which method to use
        /// </summary>
        AutoDetect
    }
}
