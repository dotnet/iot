// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// The type of package for TSL256x. This is used for the Lux calculation
    /// </summary>
    public enum PackageType
    {
        /// <summary>
        /// PAckage type CS
        /// </summary>
        PackageCs = 0,

        /// <summary>
        /// Package type CL, T and FN
        /// </summary>
        Other,
    }
}
