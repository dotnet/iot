// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Operating range
    /// </summary>
    public enum OperationRange
    {
        /// <summary>Minimum range: 5 millimeters</summary>
        Minimum = 5,
        /// <summary>Maximum range: 8 millimeters</summary>
        Maximum = 8000,
        /// <summary>Out of range</summary>
        OutOfRange = 8190
    }
}
