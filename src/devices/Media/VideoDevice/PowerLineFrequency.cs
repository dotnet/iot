// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    /// <summary>
    /// The power line frequency of a video device.
    /// </summary>
    public enum PowerLineFrequency : int
    {
        Disabled = 0,
        Frequency50Hz = 1,
        Frequency60Hz = 2,
        Auto = 3,
    }
}
