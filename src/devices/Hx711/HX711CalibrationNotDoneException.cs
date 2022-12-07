// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.HX711
{
    public class HX711CalibrationNotDoneException : Exception
    {
        private const string MESSAGE = "HX711 component need a calibration process first.";

        public HX711CalibrationNotDoneException()
        : base(message: MESSAGE) { }

        public HX711CalibrationNotDoneException(Exception inner)
            : base(message: MESSAGE, inner) { }
    }
}
