// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// Radio frequency field modes
    /// </summary>
    [Flags]
    public enum RfFieldMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0b0000_0000,

        /// <summary>
        /// Auto radio frequency CA
        /// </summary>
        AutoRFCA = 0b0000_0010,

        /// <summary>
        /// Radio frequency
        /// </summary>
        RF = 0b0000_0001,
    }
}
