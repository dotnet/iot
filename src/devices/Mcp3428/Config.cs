// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp3428
{
    /// <summary>
    /// Possible gain values of the ADC
    /// </summary>
    public enum AdcGain : byte
    { 
        /// <summary>1x gain</summary>
        X1 = 0,
        
        /// <summary>2x gain</summary>
        X2 = 1,

        /// <summary>4x gain</summary>
        X4 = 2,

        /// <summary>8x gain</summary>
        X8 = 3
    }

    /// <summary>
    /// Possible operation modes of the ADC
    /// </summary>
    [Flags]
    public enum AdcMode : byte
    {
        /// <summary>One shot mode</summary>
        OneShot = 0,

        /// <summary>Continuous mode</summary>
        Continuous = 16
    }

    /// <summary>
    /// Possible connection states for the address pins
    /// </summary>
    public enum PinState : byte
    {
        /// <summary>Low state</summary>
        Low = 0,

        /// <summary>High state</summary>
        High = 1,

        /// <summary>Floating state</summary>
        Floating = 2
    }

    /// <summary>
    /// Possible resolution values of the ADC
    /// </summary>
    [Flags]
    public enum AdcResolution : byte
    {
        /// <summary>12-bit resolution</summary>
        Bit12 = 0,

        /// <summary>14-bit resolution</summary>
        Bit14 = 4,

        /// <summary>16-bit resolution</summary>
        Bit16 = 8
    }
}
