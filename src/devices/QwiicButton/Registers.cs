//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// Register map for the Qwiic Button.
    /// </summary>
    internal enum Register : byte
    {
        ID = 0x00,
        FIRMWARE_MINOR = 0x01,
        FIRMWARE_MAJOR = 0x02,
        BUTTON_STATUS = 0x03,
        INTERRUPT_CONFIG = 0x04,
        BUTTON_DEBOUNCE_TIME = 0x05,
        PRESSED_QUEUE_STATUS = 0x07,
        PRESSED_QUEUE_FRONT = 0x08,
        PRESSED_QUEUE_BACK = 0x0C,
        CLICKED_QUEUE_STATUS = 0x10,
        CLICKED_QUEUE_FRONT = 0x11,
        CLICKED_QUEUE_BACK = 0x15,
        LED_BRIGHTNESS = 0x19,
        LED_PULSE_GRANULARITY = 0x1A,
        LED_PULSE_CYCLE_TIME = 0x1B,
        LED_PULSE_OFF_TIME = 0x1D,
        I2C_ADDRESS = 0x1F
    }
}
