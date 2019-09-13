// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// Region (Pre-Emphasis time constant)
    /// </summary>
    public enum Region
    {
        /// <summary>
        /// Pre-Emphasis time constant is 75μs
        /// </summary>
        America,

        /// <summary>
        /// Pre-Emphasis time constant is 75μs
        /// </summary>
        Japan,

        /// <summary>
        /// Pre-Emphasis time constant is 50μs
        /// </summary>
        Europe,

        /// <summary>
        /// Pre-Emphasis time constant is 50μs
        /// </summary>
        Australia,

        /// <summary>
        /// Pre-Emphasis time constant is 50μs
        /// </summary>
        China,

        /// <summary>
        /// Pre-Emphasis time constant is 50μs
        /// </summary>
        Other
    }
}
