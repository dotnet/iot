// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Store the information regarding the sensor
    /// </summary>
    public class Information
    {
        /// <summary>
        /// Module ID
        /// </summary>
        public byte ModuleId { get; set; }
        /// <summary>
        /// The revision number
        /// </summary>
        public Version Revision { get; set; }
        /// <summary>
        /// The product ID
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Raw measurement of the signal rate fixed point 400 micrometers
        /// </summary>
        internal uint SignalRateMeasFixed1104_400_Micrometers { get; set; }
        /// <summary>
        /// Raw measurement of the distance measurement fixed point 400 micrometers
        /// </summary>
        internal uint DistMeasFixed1104_400_Micrometers { get; set; }
        /// <summary>
        /// Get the offset in micrometers
        /// Formula from the official API
        /// </summary>
        public int OffsetMicrometers => SignalRateMeasFixed1104_400_Micrometers != 0 ? (((int)SignalRateMeasFixed1104_400_Micrometers - (400 << 4) * 1000) >> 4) * -1 : 0;
        /// <summary>
        /// Get the signal rate measurement fixed point 400 micrometers
        /// Formula from the official API
        /// </summary>
        public uint SignalRateMeasuementFixed400Micrometers => SignalRateMeasFixed1104_400_Micrometers << 9;
    }
}
