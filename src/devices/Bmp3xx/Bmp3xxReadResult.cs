// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// Contains a measurement result of a BMP3xx sensor.
    /// </summary>
    public class Bmp3xxReadResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp3xxReadResult"/> class.
        /// </summary>
        /// <param name="temperature">The measured temperature, or null if no valid value was available.</param>
        /// <param name="pressure">The measured pressure, or null if no valid value was available.</param>
        public Bmp3xxReadResult(Temperature? temperature, Pressure? pressure)
        {
            Temperature = temperature;
            Pressure = pressure;
        }

        /// <summary>
        /// Gets the measured temperature, or null if no valid value was available.
        /// </summary>
        public Temperature? Temperature { get; }

        /// <summary>
        /// Gets the measured pressure, or null if no valid value was available.
        /// </summary>
        public Pressure? Pressure { get; }
    }
}
