// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max31865
{
    /// <summary>
    /// Notch frequencies for the noise rejection filter
    /// </summary>
    public enum ConversionFilterMode : byte
    {
        /// <summary>
        /// Reject 50Hz and its harmonics
        /// </summary>
        Filter50Hz = 65,

        /// <summary>
        /// Reject 60Hz and its harmonics
        /// </summary>
        Filter60Hz = 55
    }
}
