// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.Bmxx80.ReadResult
{
    /// <summary>
    /// Contains a measurement result of a Bme280 sensor.
    /// </summary>
    public class Bme280ReadResult : Bmp280ReadResult
    {
        /// <summary>
        /// Collected humidity measurement.
        /// </summary>
        public Ratio? Humidity { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme280ReadResult"/> class.
        /// </summary>
        /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
        /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
        /// <param name="humidity">The humidity measurement.</param>
        public Bme280ReadResult(Temperature? temperature, Pressure? pressure, Ratio? humidity)
            : base(temperature, pressure)
        {
            Humidity = humidity;
        }
    }
}
