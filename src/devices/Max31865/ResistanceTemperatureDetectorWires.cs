// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max31865
{
    /// <summary>
    /// Number of platinum thermometer sensor wires
    /// </summary>
    public enum ResistanceTemperatureDetectorWires : byte
    {
        /// <summary>
        /// Two wire platinum thermometer sensor
        /// </summary>
        TwoWire = 2,

        /// <summary>
        /// Three wire platinum thermometer sensor
        /// </summary>
        ThreeWire = 3,

        /// <summary>
        /// Four wire platinum thermometer sensor
        /// </summary>
        FourWire = 4
    }
}
