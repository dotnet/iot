// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// ADX1345 Data
    /// </summary>
    public class Acceleration
    {
        /// <summary>
        /// X-axis acceleration (G)
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Y-axis acceleration (G)
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Z-axis acceleration (G)
        /// </summary>
        public double Z { get; set; }
    }
}
