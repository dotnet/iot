// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Card
{
    /// <summary>
    /// All supported version for the NFC reader
    /// TODO: if more readers appear, this can be for sure place
    /// in common and more modes can be added
    /// </summary>
    [Flags]
    public enum VersionSupported
    {
        Iso18092 = 0b0000_0100,
        IsoIec14443TypeB = 0b0000_0010,
        IsoIec14443TypeA = 0b0000_0001
    }
}
