// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Minimum and maximum values for a specific mode type
    /// </summary>
    public struct MinimumMaximumValues
    {
        /// <summary>
        /// Type of values
        /// </summary>
        public TypeValues TypeValues { get; set; }

        /// <summary>
        /// Minimum value
        /// </summary>
        public int MinimumValue { get; set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        public int MaximumValue { get; set; }
    }
}
