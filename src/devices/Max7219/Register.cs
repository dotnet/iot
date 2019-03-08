// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Max7219
{
    /// <summary>
    /// Register of the Max7219 Display Driver
    /// </summary>
    internal enum Register : byte
    {
        NOOP = 0x0,
        DIGIT0 = 0x1,
        DIGIT1 = 0x2,
        DIGIT2 = 0x3,
        DIGIT3 = 0x4,
        DIGIT4 = 0x5,
        DIGIT5 = 0x6,
        DIGIT6 = 0x7,
        DIGIT7 = 0x8,
        DECODEMODE = 0x9,
        INTENSITY = 0xA,
        SCANLIMIT = 0xB,
        SHUTDOWN = 0xC,
        DISPLAYTEST = 0xF
    }
}
