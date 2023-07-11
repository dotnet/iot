// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3DhAccelerometer
{
    /// <summary>
    /// Data rate selection
    /// </summary>
    public enum DataRate : byte
    {
        /// <summary>
        /// Power down mode
        /// </summary>
        PowerDownMode = 0b0000,

        /// <summary>
        /// Data rate 1Hz
        /// </summary>
        DataRate1Hz = 0b0001,

        /// <summary>
        /// Data rate 10Hz
        /// </summary>
        DataRate10Hz = 0b0010,

        /// <summary>
        /// Data rate 25Hz
        /// </summary>
        DataRate25Hz = 0b0011,

        /// <summary>
        /// Data rate 50Hz
        /// </summary>
        DataRate50Hz = 0b0100,

        /// <summary>
        /// Data rate 100Hz
        /// </summary>
        DataRate100Hz = 0b0101,

        /// <summary>
        /// Data rate 200Hz
        /// </summary>
        DataRate200Hz = 0b0110,

        /// <summary>
        /// Data rate 400Hz
        /// </summary>
        DataRate400Hz = 0b0111,

        /// <summary>
        /// Data rate 1.6kHz - this data rate mode is only applicable for LowPowerMode
        /// </summary>
        LowPowerMode1600Hz = 0b1000,

        /// <summary>
        /// Data rate, for HighResolutionMode or NormalMode 1.344kHz, for LowPowerMode 5.376kHz
        /// </summary>
        HighResolutionNormal1344HzOrLowPowerMode5376Hz = 0b1001,
    }
}
