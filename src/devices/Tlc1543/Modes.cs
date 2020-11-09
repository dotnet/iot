// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tlc1543
{
    /// <summary>
    /// Available operational transfer modes for Tlc1543
    /// <remarks>
    /// <br>For more information see Table 1 in datasheet</br>
    /// </remarks>
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// <br>Fast mode</br>
        /// <br>10 I/O Clocks</br>
        /// <br>Using Chip Select</br>
        /// </summary>
        Mode1 = 1,

        /// <summary>
        /// <br>Fast mode</br>
        /// <br>10 I/O Clocks</br>
        /// <br>Requires End Of Conversion pin being connected</br>
        /// <br>Otherwise takes max conversion time between measurements</br>
        /// </summary>
        Mode2 = 2,

        /// <summary>
        /// <br>Fast mode</br>
        /// <br>11 to 16 I/O Clocks</br>
        /// <br>Using Chip Select</br>
        /// </summary>
        Mode3 = 3,

        /// <summary>
        /// <br>Fast mode</br>
        /// <br>16 I/O Clocks</br>
        /// <br>Requires End Of Conversion pin being connected</br>
        /// <br>Otherwise takes max conversion time between measurements</br>
        /// </summary>
        Mode4 = 4,

        /// <summary>
        /// <br>Slow mode</br>
        /// <br>11 to 16 I/O Clocks</br>
        /// <br>Using Chip Select</br>
        /// </summary>
        Mode5 = 5,

        /// <summary>
        /// <br>Slow mode</br>
        /// <br>16 I/O Clocks</br>
        /// <br>Requires End Of Conversion pin being connected</br>
        /// <br>Otherwise takes max conversion time between measurements</br>
        /// </summary>
        Mode6 = 6,
    }
}
