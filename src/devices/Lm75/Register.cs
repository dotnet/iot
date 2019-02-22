// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lm75
{
    public enum Register : byte
    {
        LM_TEMP = 0x00,
        LM_CONFIG = 0x01,
        LM_THYST = 0x02,
        LM_TOS = 0x03
    }
}