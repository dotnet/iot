// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// A combi mode is a list of available modes
    /// </summary>
    public struct CombiModes
    {
        /// <summary>
        /// Gets or sets the combi number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets a mode
        /// </summary>
        public int[] Modes { get; set; }
    }
}
