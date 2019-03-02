// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Si7021 Register
    /// </summary>
    internal enum Register : byte
    {
        SI_TEMP = 0xE3,
        SI_HUMI = 0xE5,
        SI_RESET = 0xFE,
        SI_REVISION_MSB = 0x84,
        SI_REVISION_LSB = 0xB8,
        SI_USER_REG1_WRITE = 0xE6,
        SI_USER_REG1_READ = 0xE7,
    }
}
