// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Led blink pattern
    /// </summary>
    /// <param name="Led">Led designator.</param>
    /// <param name="BlinkIndefinite">Blink indefinite.</param>
    /// <param name="Count">Number of blinks between [1 - 254].</param>
    /// <param name="ColorFirstPeriod">Color for first period of blink.</param>
    /// <param name="FirstPeriod">Duration of first blink period in milliseconds between [10 – 2550].</param>
    /// <param name="ColorSecondPeriod">Color for second period of blink.</param>
    /// <param name="SecondPeriod">Duration of second blink period in milliseconds between [10 – 2550].</param>
    public record LedBlink(Led Led, bool BlinkIndefinite, byte Count, Color ColorFirstPeriod, TimeSpan FirstPeriod, Color ColorSecondPeriod, TimeSpan SecondPeriod);
}
