// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// Integration time
    /// </summary>
    public enum IntegrationTime : byte
    {
        /// <summary>
        /// Integration time 13.7 milliseconds
        /// </summary>
        Integration13_7Milliseconds = 0,

        /// <summary>
        /// Integration time 101 milliseconds
        /// </summary>
        Integration101Milliseconds = 1,

        /// <summary>
        /// Integration time 402 milliseconds
        /// </summary>
        Integration402Milliseconds = 2,

        /// <summary>
        /// Manual time integration
        /// </summary>
        Manual = 3,
    }
}
