// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// Radio frequence field modes
    /// </summary>
    [Flags]
    public enum RfFieldMode
    {
        None = 0b0000_0000,
        AutoRFCA = 0b0000_0010,
        RF = 0b0000_0001,
    }
}
