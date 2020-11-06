// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Comparator Queue.
    /// </summary>
    public enum ComparatorQueue
    {
        /// <summary>Assert after one</summary>
        AssertAfterOne = 0x00,

        /// <summary>Assert after two</summary>
        AssertAfterTwo = 0x01,

        /// <summary>Assert after four</summary>
        AssertAfterFour = 0x02,

        /// <summary>Disable</summary>
        Disable = 0x03
    }
}
