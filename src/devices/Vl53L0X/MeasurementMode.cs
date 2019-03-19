// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// The measurement mode
    /// Continuous measurement is processed in the sensor and readings are 
    /// more reliable than the Single measurement mode
    /// </summary>
    public enum MeasurementMode
    {
        Continuous = 0,
        Single
    }
}
