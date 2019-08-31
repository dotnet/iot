// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Axis map
    /// Orientation map is from documentation section 3.4
    /// The dot is the one on the chip
    ///                    | Z axis
    ///                    |
    ///                    |   / X axis
    ///                ____|__/____
    ///   Y axis     / *   | /    /|
    ///   _________ /______|/    //
    ///            /___________ //
    ///            |____________|/
    /// </summary>
    public enum AxisMap
    {
        /// <summary>X coordinate</summary>
        X = 0x00,
        /// <summary>Y coordinate</summary>
        Y = 0x01,
        /// <summary>Z coordinate</summary>
        Z = 0x02,
    }

    /// <summary>
    /// Axis signs
    /// </summary>
    public enum AxisSign
    {
        /// <summary>Positive axis sign</summary>
        Positive = 0x00,
        /// <summary>Negative axis sign</summary>
        Negative = 0x01,
    }

    /// <summary>
    /// Axis setting for a specific axis
    /// </summary>
    public class AxisSetting
    {
        /// <summary>
        /// Axis map
        /// </summary>
        public AxisMap Axis { get; set; }

        /// <summary>
        /// Axis sign
        /// </summary>
        public AxisSign Sign { get; set; }
    }
}
