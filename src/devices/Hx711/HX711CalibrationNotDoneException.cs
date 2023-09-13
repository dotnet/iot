// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Exception thorw if Hx711 miss calibration process
    /// </summary>
    public class Hx711CalibrationNotDoneException : Exception
    {
        private new const string Message = "Hx711 component need a calibration process first.";

        /// <summary>
        /// Initializes a new instance of the <see cref="Hx711CalibrationNotDoneException"/> class.
        /// </summary>
        public Hx711CalibrationNotDoneException()
        : base(message: Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hx711CalibrationNotDoneException"/> class.
        /// </summary>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public Hx711CalibrationNotDoneException(Exception inner)
            : base(message: Message, inner)
        {
        }
    }
}
