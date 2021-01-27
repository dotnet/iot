// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Exception thrown when trying to access certain values without initializing the sensor.
    /// </summary>
    public class SensorNotInitializedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorNotInitializedException"/> class.
        /// </summary>
        public SensorNotInitializedException()
            : base("Sensor has not yet been initialized. Please call SetMode() to initialize it.")
        {
        }
    }
}
