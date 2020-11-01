// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Analog
{
    /// <summary>
    /// Gives the reason why a new value was provided
    /// </summary>
    public enum TriggerReason
    {
        /// <summary>
        /// The reason for the new message is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// A new value is available
        /// </summary>
        NewMeasurement,

        /// <summary>
        /// A new value is read with a specific frequency
        /// </summary>
        Timed,

        /// <summary>
        /// A value is provided when certain thresholds are exceeded
        /// </summary>
        LimitExceeded
    }
}
