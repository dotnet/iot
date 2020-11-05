// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Store the information regarding the sensor
    /// </summary>
    public class Information
    {
        /// <summary>
        /// Creates an Information object.
        /// </summary>
        /// <param name="moduleId">Module ID/</param>
        /// <param name="revision">The revision number.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="signalRateMeasFixed1104_400_Micrometers">Raw measurement of the signal rate fixed point 400 micrometers.</param>
        /// <param name="distMeasFixed1104_400_Micrometers">Raw measurement of the distance measurement fixed point 400 micrometers.</param>
        public Information(byte moduleId, Version revision, string productId, uint signalRateMeasFixed1104_400_Micrometers, uint distMeasFixed1104_400_Micrometers)
        {
            ModuleId = moduleId;
            Revision = revision;
            ProductId = productId;
            SignalRateMeasFixed1104_400_Micrometers = signalRateMeasFixed1104_400_Micrometers;
            DistMeasFixed1104_400_Micrometers = distMeasFixed1104_400_Micrometers;
        }

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
        private uint SignalRateMeasFixed1104_400_Micrometers { get; set; }

        /// <summary>
        /// Raw measurement of the distance measurement fixed point 400 micrometers
        /// </summary>
        private uint DistMeasFixed1104_400_Micrometers { get; set; }

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
