// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The Radio Frequence configuration timeout
    /// </summary>
    public enum RFConfigurationTimeout
    {
        NoTimeout = 0x00,
        Timeout100µs = 0x01,
        Timeout200µs = 0x02,
        Timeout400µs = 0x03,
        Timeout800µs = 0x04,
        Timeout1Dot6ms = 0x05,
        Timeout3Dot2ms = 0x06,
        Timeout6Dot4ms = 0x07,
        Timeout12Dot8ms = 0x08,
        Timeout25Dot6ms = 0x09,
        Timeout51Dot2ms = 0x0A,
        Timeout102Dot4ms = 0x0B,
        Timeout204Dot8ms = 0x0C,
        Timeout409Dot6ms = 0x0D,
        Timeout819Dot2ms = 0x0E,
        Timeout1Dot64sec = 0x0F,
        Timeout3Dot28sec = 0x10,
    }
}
