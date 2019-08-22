// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Kt0803
{
    /// <summary>
    /// Kt0803 Register
    /// </summary>
    internal enum Register : byte
    {
        KT_CHSEL = 0x00,
        KT_CONFIG1 = 0x01,
        KT_CONFIG2 = 0x02,
        KT_CONFIG3 = 0x04,
        KT_CONFIG4 = 0x0B,
        KT_CONFIG6 = 0x0E,
        KT_CONFIG10 = 0x13,
    }
}
