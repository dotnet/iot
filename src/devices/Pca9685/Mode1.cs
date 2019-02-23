// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pca9685
{
    /// <summary>
    /// Values for MODE1 register
    /// </summary>
    internal enum Mode1 : byte
    {
        RESTART = 0b10000000,   // Bit 7
        EXTCLK  = 0b01000000,   // Bit 6
        AI      = 0b00100000,   // Bit 5
        SLEEP   = 0b00010000,   // Bit 4
        SUB1    = 0b00001000,   // Bit 3
        SUB2    = 0b00000100,   // Bit 2
        SUB3    = 0b00000010,   // Bit 1
        ALLCALL = 0x00000001,   // Bit 0
    }
}
