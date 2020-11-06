// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max44009
{
    /// <summary>
    /// Measurement Cycle
    /// </summary>
    public enum IntegrationTime : byte
    {
        /// <summary>
        /// 800ms
        /// </summary>
        Time800 = 0b000,

        /// <summary>
        /// 400ms
        /// </summary>
        Time400 = 0b001,

        /// <summary>
        /// 200ms
        /// </summary>
        Time200 = 0b010,

        /// <summary>
        /// 100ms
        /// </summary>
        Time100 = 0b011,

        /// <summary>
        /// 50ms
        /// </summary>
        Time050 = 0b100,

        /// <summary>
        /// 25ms
        /// </summary>
        Time025 = 0b101,

        /// <summary>
        /// 12.5ms
        /// </summary>
        Time012_5 = 0b110,

        /// <summary>
        /// 6.25ms
        /// </summary>
        Time006_25 = 0b111
    }
}
