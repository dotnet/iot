// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// Radio frequency timeouts
    /// </summary>
    public enum RfTimeout
    {
        None = 0x00,
        T100MicroSeconds = 0x01,
        T200MicroSeconds = 0x02,
        T400MicroSeconds = 0x03,
        T800MicroSeconds = 0x04,
        T1600MicroSeconds = 0x05,
        T3200MicroSeconds = 0x06,
        T6400MicroSeconds = 0x07,
        T12800MicroSeconds = 0x08,
        T25600MicroSeconds = 0x09,
        T51200MicroSeconds = 0x0A,
        T102400MicroSeconds = 0x0B,
        T204800MicroSeconds = 0x0C,
        T409600MicroSeconds = 0x0D,
        T819200MicroSeconds = 0x0E,
        T1640MilliSeconds = 0x0F,
        T3280MilliSeconds = 0x10,
    }
}
