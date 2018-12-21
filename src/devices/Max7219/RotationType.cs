// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Max7219
{
  
    /// <summary>
    /// Rotation if several displays are rotated in a row.
    /// </summary>
    public enum RotationType
    {
        /// <summary>
        /// No rotation needed
        /// </summary>
        None,
        /// <summary>
        /// each device is turned 90 degree to the right
        /// </summary>
        Right,
        /// <summary>
        /// each device is turned 90 degree to the left
        /// </summary>
        Half,
        /// <summary>
        /// each device is turned by 180 degree
        /// </summary>
        Left,
    }
}