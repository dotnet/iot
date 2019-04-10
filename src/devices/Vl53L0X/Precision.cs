// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Sensor have multiple modes, you can select one of the 
    /// predefined mode using the SetPrecision method
    /// </summary>
    public enum Precision
    {
        ShortRange = 0,
        LongRange,
        HighPrecision,
    }
}
