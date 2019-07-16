// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The P7 state
    /// </summary>
    [Flags]
    public enum P7
    {
        P72 = 0b0000_0100,
        P71 = 0b0000_0010,
    }
}
