// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Contains a measurement result of a Bmxx80 series sensor.
    /// </summary>
    public class Bmxx80ReadResult
    {
        /// <summary>
        /// Collected temperature measurement. Null if no measurement was performed.
        /// </summary>
        public Temperature? Temperature;

        /// <summary>
        /// Collected pressure measurement. Null if no measurement was performed.
        /// </summary>
        public Pressure? Pressure;

        /// <summary>
        /// Collected humidity measurement. Null if no measurement was performed.
        /// </summary>
        public double? Humidity;

        /// <summary>
        /// Collected gas resistance measurement. Null if no measurement was performed.
        /// </summary>
        public double? GasResistance;
    }
}
