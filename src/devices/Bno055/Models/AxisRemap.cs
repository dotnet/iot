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
        X = 0x00,
        Y = 0x01,
        Z = 0x02,
    }

    /// <summary>
    /// Axis signs
    /// </summary>
    public enum AxisSign
    {
        Positive = 0x00,
        Negative = 0x01,
    }

    /// <summary>
    /// Axis setting for a specific axis
    /// </summary>
    public class AxisSetting
    {
        public AxisMap Axis { get; set; }
        public AxisSign Sign { get; set; }
    }
}
