// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton.RegisterMapping
{
    /// <summary>
    /// Register map for the Qwiic Button.
    /// </summary>
    internal enum Register : byte
    {
        Id = 0x00,
        FirmwareMinor = 0x01,
        FirmwareMajor = 0x02,
        ButtonStatus = 0x03,
        InterruptConfig = 0x04,
        ButtonDebounceTime = 0x05,
        PressedQueueStatus = 0x07,
        PressedQueueFront = 0x08,
        PressedQueueBack = 0x0C,
        ClickedQueueStatus = 0x10,
        ClickedQueueFront = 0x11,
        ClickedQueueBack = 0x15,
        LedBrightness = 0x19,
        LedPulseGranularity = 0x1A,
        LedPulseCycleTime = 0x1B,
        LedPulseOffTime = 0x1D,
        I2cAddress = 0x1F
    }
}
