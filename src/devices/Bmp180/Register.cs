// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmp180
{
    internal enum Register : byte
    {
         AC1 = 0xAA,
         AC2 = 0xAC,
         AC3 = 0xAE,
         AC4 = 0xB0,
         AC5 = 0xB2,
         AC6 = 0xB4,

         B1 = 0xB6,
         B2 = 0xB8,

         MB = 0xBA,
         MC = 0xBC,
         MD = 0xBE,

         CONTROL = 0xF4,
         READTEMPCMD = 0x2E,
         READPRESSURECMD = 0x34,
         TEMPDATA = 0xF6,
         PRESSUREDATA = 0xF6,
    }
}
