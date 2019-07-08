// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp3428
{
    /// <summary>
    /// Possible gain values of the ADC
    /// </summary>
    public enum AdcGain : byte { X1 = 0, X2 = 1, X4 = 2, X8 = 3 }

    /// <summary>
    /// Possible operation modes of the ADC
    /// </summary>
    [System.Flags]
    public enum AdcMode : byte { OneShot = 0, Continuous = 16 }

    /// <summary>
    /// Possible connection states for the address pins
    /// </summary>
    public enum PinState : byte { Low = 0, High = 1, Floating = 2 }

    /// <summary>
    /// Possible resolution values of the ADC
    /// </summary>
    [System.Flags]
    public enum AdcResolution : byte { Bit12 = 0, Bit14 = 4, Bit16 = 8 }
}
