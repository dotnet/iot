// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22hb
{
    /// <summary>
    /// Output data rate selection
    /// Table 17. Output data rate bit configurations p37
    /// </summary>
    public enum OutputRate
    {
        /// <summary>
        /// Power down / one-shot mode enabled
        /// </summary>
        PowerDownMode = 0b000,

        /// <summary>
        /// Data rate 1Hz
        /// </summary>
        DataRate1Hz = 0b001,

        /// <summary>
        /// Data rate 10Hz
        /// </summary>
        DataRate10Hz = 0b010,

        /// <summary>
        /// Data rate 25Hz
        /// </summary>
        DataRate25Hz = 0b011,

        /// <summary>
        /// Data rate 50Hz
        /// </summary>
        DataRate50Hz = 0b100,

        /// <summary>
        /// Data rate 75Hz
        /// </summary>
        DataRate75Hz = 0b0101
    }
}
