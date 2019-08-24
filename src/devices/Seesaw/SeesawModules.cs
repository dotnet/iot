// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        public enum SeesawModule : byte
        {
            Status = 0x00,
            Gpio = 0x01,
            Sercom0 = 0x02,
            Timer = 0x08,
            Adc = 0x09,
            Dac = 0x0A,
            Interrupt = 0x0B,
            Dap = 0x0C,
            Eeprom = 0x0D,
            Neopixel = 0x0E,
            Touch = 0x0F,
            Keypad = 0x10,
            Encoder = 0x11
        };

        protected enum SeesawFunction : byte
        {
            // status functions
            StatusHwId = 0x01,
            StatusVersion = 0x02,
            StatusOptions = 0x03,
            StatusTemp = 0x04,
            StatusSwrst = 0x7F,

            // GPIO functions
            GpioDirsetBulk = 0x02,
            GpioDirclrBulk = 0x03,
            GpioBulk = 0x04,
            GpioBulkSet = 0x05,
            GpioBulkClr = 0x06,
            GpioBulkToggle = 0x07,
            GpioIntenset = 0x08,
            GpioIntenclr = 0x09,
            GpioIntflag = 0x0A,
            GpioPullenset = 0x0B,
            GpioPullenclr = 0x0C,

            // ADC Functions
            AdcStatus = 0x00,
            AdcInten = 0x02,
            AdcIntenclr = 0x03,
            AdcWinmode = 0x04,
            AdcWinthresh = 0x05,
            AdcChannelOffset = 0x07,

            // EEProm Addresses
            EepromI2cAddr = 0x3F,

            // touch functions
            TouchChannelOffset = 0x10,
        };
    }
}
