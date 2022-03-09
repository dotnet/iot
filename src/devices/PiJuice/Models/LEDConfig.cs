// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Led configuration
    /// </summary>
    /// <param name="Led">Led designator</param>
    /// <param name="LedFunction">Led function type</param>
    /// <param name="Color">Color for Led.
    /// If LedFunction is ChargeStatus
    /// Red - parameter defines color component level of red below 15%
    /// Green - parameter defines color component charge level over 50%
    /// Blue - parameter defines color component for charging(blink) and fully charged states(constant)
    /// Red Led and Green Led will show the charge status between 15% - 50%
    /// </param>
    public record LedConfig(Led Led, LedFunction LedFunction, Color Color);
}
