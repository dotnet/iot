// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Max44009
{
    /// <summary>
    /// MAX44009 Working Mode
    /// </summary>
    public enum WorkingMode
    {
        /// <summary>
        /// Measure lux intensity only once every 800ms regardless of integration time.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Measure lux intensity continuously.
        /// If integration time is 6.25ms, readings are taken every 6.25ms. If integration time is 800ms, readings are taken every 800ms.
        /// </summary>
        Continuous = 1
    }
}
