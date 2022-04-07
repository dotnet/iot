// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// When running a motor to a position, the way to go.
    /// </summary>
    public enum PositionWay
    {
        /// <summary>Shortest way</summary>
        Shortest,

        /// <summary>Clockwise way</summary>
        Clockwise,

        /// <summary>Anti clockwise way</summary>
        AntiClockwise,
    }
}
