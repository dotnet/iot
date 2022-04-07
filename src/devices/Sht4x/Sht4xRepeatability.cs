// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Desired repeatability of relative humidity measurement.
    /// </summary>
    public enum Sht4xRepeatability
    {
        /// <summary>
        /// 0.25% RH error
        /// </summary>
        Low,

        /// <summary>
        /// 0.15% RH error
        /// </summary>
        Medium,

        /// <summary>
        /// 0.08% RH error
        /// </summary>
        High
    }
}
