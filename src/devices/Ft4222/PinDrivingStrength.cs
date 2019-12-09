// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Ft4222
{
    /// <summary>
    /// Intensity for SPI in Milli Amperes
    /// </summary>
    internal enum PinDrivingStrength
    {
        /// <summary>
        /// 4 Milli Amperes
        /// </summary>
        Intensity4Ma = 0,
        /// <summary>
        /// 8 Milli Amperes
        /// </summary>
        Intensity8Ma,
        /// <summary>
        /// 12 Milli Amperes
        /// </summary>
        Intensity12Ma,
        /// <summary>
        /// 16 Milli Amperes
        /// </summary>
        Intensity16Ma,
    }
}
