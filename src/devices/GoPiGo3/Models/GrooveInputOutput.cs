// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The setup of the Groove element
    /// </summary>
    public enum GrooveInputOutput
    {
        InputDigital = 0,
        OutputDigital,
        InputDigitalPullUp,
        InputDigitalPullDown,
        InputAnalog,
        OutputPwm,
        InputAnalogPullUp,
        InputAnalogPullDown
    }
}
