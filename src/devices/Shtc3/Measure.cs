// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;

namespace Iot.Device.Shtc3
{
    /// <summary>
    /// Shtc3 measure
    /// </summary>
    public readonly struct Measure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Measure"/> struct.
        /// </summary>
        /// <param name="temperature">Temperature</param>
        /// <param name="humidity">Humidity</param>
        public Measure(Temperature temperature, double humidity)
        {
            Temperature = temperature;
            Humidity = humidity;
        }

        /// <summary>
        /// Temperature
        /// </summary>
        public Temperature Temperature { get; }

        /// <summary>
        /// Humidity
        /// </summary>
        public double Humidity { get;  }
    }

}
