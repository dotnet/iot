// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// The minimum operation range is 5 millimeters
    /// The maximum operation range is 8 meters, the sensor needs to be in long range mode
    /// When out of range, the sensor will send 8190 
    /// </summary>
    public enum OperationRange
    {
        Minimum = 5,
        Maximum = 8000,
        OutOfRange = 8190
    }
}
