// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// Radio frequence timeouts
    /// </summary>
    public enum RfTimeout
    {
        None = 0x00,
        T100µs = 0x01,
        T200µs = 0x02,
        T400µs = 0x03,
        T800µs = 0x04,
        T1600µs = 0x05,
        T3200µs = 0x06,
        T6400µs = 0x07,
        T12800µs = 0x08,
        T25600µs = 0x09,
        T51200µs = 0x0A,
        T102400µs = 0x0B,
        T204800µs = 0x0C,
        T409600µs = 0x0D,
        T819200µs = 0x0E,
        T1640ms = 0x0F,
        T3280ms = 0x10,
    }
}
