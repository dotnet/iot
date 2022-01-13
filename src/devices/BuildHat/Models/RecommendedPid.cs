// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Recommended PID settings
    /// </summary>
    public struct RecommendedPid
    {
        /// <summary>
        /// Gets or sets the PID1
        /// </summary>
        public int Pid1 { get; set; }

        /// <summary>
        /// Gets or sets the PID2
        /// </summary>
        public int Pid2 { get; set; }

        /// <summary>
        /// Gets or sets the PID4
        /// </summary>
        public int Pid3 { get; set; }

        /// <summary>
        /// Gets or sets the PID5
        /// </summary>
        public int Pid4 { get; set; }
    }
}
