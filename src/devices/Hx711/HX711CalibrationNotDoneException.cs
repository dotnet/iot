// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.HX711
{
    /// <summary>
    /// Exception thorw if Hx711 miss calibration process
    /// </summary>
    public class HX711CalibrationNotDoneException : Exception
    {
        private const string MESSAGE = "HX711 component need a calibration process first.";

        /// <summary>
        /// Initializes a new instance of the <see cref="HX711CalibrationNotDoneException"/> class.
        /// </summary>
        public HX711CalibrationNotDoneException()
        : base(message: MESSAGE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HX711CalibrationNotDoneException"/> class.
        /// </summary>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public HX711CalibrationNotDoneException(Exception inner)
            : base(message: MESSAGE, inner)
        {
        }
    }
}
