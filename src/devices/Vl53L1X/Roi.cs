// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

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
        /// Constructor.
        /// </summary>
        /// <param name="width">Width of the ROI. Must be between 4 and 16.</param>
        /// <param name="height">Height of the ROI. Must be between 4 and 16.</param>
        public Roi(ushort width, ushort height)
        {
            if (width < 4 || width > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Must be between 4 and 16.");
            }

            if (height < 4 || height > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Must be between 4 and 16.");
            }

            Width = width;
            Height = height;
        }
    }
}
