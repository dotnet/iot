// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the set of ranges for the ambient light sensor illuminance
    /// measurement.
    /// </summary>
    public enum AlsRange
    {
        /// <summary>
        /// Range is 0 to 819.2 lux
        /// </summary>
        Range_819,

        /// <summary>
        /// Range is 0 to 1638.4 lux
        /// </summary>
        Range_1638,

        /// <summary>
        /// Range is 0 to 3276.7 lux
        /// </summary>
        Range_3276,

        /// <summary>
        /// Range is 0 to 6553.5 lux
        /// </summary>
        Range_6553,
    }
}
