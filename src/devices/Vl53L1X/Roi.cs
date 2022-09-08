// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// Holds the ROI information of the device
    /// </summary>
    public class Roi
    {
        /// <summary>
        /// Width of the ROI
        /// </summary>
        public ushort Width { get; }

        /// <summary>
        /// Height of the ROI
        /// </summary>
        public ushort Height { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">Width of the ROI</param>
        /// <param name="height">Height of the ROI</param>
        public Roi(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }
    }
}
