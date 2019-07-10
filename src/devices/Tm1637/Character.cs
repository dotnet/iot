// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Tm1637
{
    /// <summary>
    /// Pre build character to be displayed including the potential dot
    /// Please note that depending on your LCD display you may not have the dot present
    /// </summary>
    public class Character
    {
        public Display Char { get; set; }
        public bool Dot { get; set; }
    }
}
