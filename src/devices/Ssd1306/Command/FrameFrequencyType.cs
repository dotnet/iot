// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public enum FrameFrequencyType
    {
        Frames2 = 0x07,
        Frames3 = 0x04,
        Frames4 = 0x05,
        Frames5 = 0x00,
        Frames25 = 0x06,
        Frames64 = 0x01,
        Frames128 = 0x02,
        Frames256 = 0x03
    }
}
